using AmeisenBotX.Core.Battleground;
using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Character.Inventory;
using AmeisenBotX.Core.Character.Inventory.Objects;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.CombatLog;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Persistence;
using AmeisenBotX.Core.Event;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.Jobs;
using AmeisenBotX.Core.Movement;
using AmeisenBotX.Core.Movement.Settings;
using AmeisenBotX.Core.OffsetLists;
using AmeisenBotX.Core.Personality;
using AmeisenBotX.Core.StateMachine;
using AmeisenBotX.Core.StateMachine.CombatClasses;
using AmeisenBotX.Core.StateMachine.CombatClasses.Jannis;
using AmeisenBotX.Core.StateMachine.States;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using AmeisenBotX.Memory;
using AmeisenBotX.Memory.Win32;
using AmeisenBotX.Pathfinding;
using AmeisenBotX.Pathfinding.Objects;
using Microsoft.CSharp;
using Newtonsoft.Json;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Timers;
using Timer = System.Timers.Timer;

namespace AmeisenBotX.Core
{
    public class AmeisenBot
    {
        private double currentExecutionMs;
        private int stateMachineTimerBusy;

        public AmeisenBot(string botDataPath, string accountName, AmeisenBotConfig config)
        {
            SetupLogging(botDataPath, accountName);
            AmeisenLogger.Instance.Log("AmeisenBot", "AmeisenBot starting...", LogLevel.Master);

            string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            AmeisenLogger.Instance.Log("AmeisenBot", $"version: {version}", LogLevel.Master);

            Config = config;

            AccountName = accountName;
            AmeisenLogger.Instance.Log("AmeisenBot", $"AccountName: {botDataPath}", LogLevel.Master);

            BotDataPath = botDataPath;
            AmeisenLogger.Instance.Log("AmeisenBot", $"BotDataPath: {botDataPath}", LogLevel.Verbose);

            CurrentExecutionMs = 0;
            CurrentExecutionCount = 0;
            stateMachineTimerBusy = 0;

            StateMachineTimer = new Timer(config.StateMachineTickMs);
            StateMachineTimer.Elapsed += StateMachineTimerTick;

            OffsetList = new OffsetList335a();
            AmeisenLogger.Instance.Log("AmeisenBot", $"Using OffsetList: {OffsetList.GetType().ToString()}", LogLevel.Master);

            XMemory = new XMemory();
            BotCache = new InMemoryBotCache(Path.Combine(BotDataPath, accountName, "cache.bin"));
            BotPersonality = new BotPersonality(Path.Combine(BotDataPath, accountName, "personality.bin"));
            CombatLogParser = new CombatLogParser(BotCache);
            ObjectManager = new ObjectManager(XMemory, OffsetList, BotCache);
            HookManager = new HookManager(XMemory, OffsetList, ObjectManager, BotCache);
            ObjectManager.HookManager = HookManager;
            CharacterManager = new CharacterManager(XMemory, config, OffsetList, ObjectManager, HookManager);
            EventHookManager = new EventHookManager(HookManager);
            PathfindingHandler = new NavmeshServerClient(Config.NavmeshServerIp, Config.NameshServerPort);
            MovementSettings = new MovementSettings();
            MovemenEngine = new SmartMovementEngine(
                () => ObjectManager.Player.Position,
                () => ObjectManager.Player.Rotation,
                CharacterManager.MoveToPosition,
                (Vector3 start, Vector3 end) => PathfindingHandler.GetPath(ObjectManager.MapId, start, end),
                CharacterManager.Jump,
                ObjectManager,
                MovementSettings);
            BattlegroundEngine = new BattlegroundEngine(HookManager, ObjectManager, MovemenEngine);
            JobEngine = new JobEngine(ObjectManager, MovemenEngine, HookManager, CharacterManager);

            if (!Directory.Exists(BotDataPath))
            {
                Directory.CreateDirectory(BotDataPath);
                AmeisenLogger.Instance.Log("AmeisenBot", $"Creating folder {botDataPath}", LogLevel.Verbose);
            }

            if (config.UseBuiltInCombatClass)
            {
                LoadDefaultCombatClass();
            }
            else
            {
                LoadCustomCombatClass();
            }

            // if a combatclass specified an ItemComparator
            // use it instead of the default one
            if (CombatClass?.ItemComparator != null)
            {
                CharacterManager.ItemComparator = CombatClass.ItemComparator;
            }

            StateMachine = new AmeisenBotStateMachine
            (
                BotDataPath,
                WowProcess,
                Config,
                XMemory,
                OffsetList,
                ObjectManager,
                CharacterManager,
                HookManager,
                EventHookManager,
                BotCache,
                PathfindingHandler,
                MovemenEngine,
                MovementSettings,
                CombatClass,
                BattlegroundEngine,
                JobEngine
            );

            StateMachine.OnStateMachineStateChanged += HandleLoadWowPosition;
        }

        public delegate void CombatClassCompilationStatus(bool succeeded, string heading, string message);

        public event CombatClassCompilationStatus OnCombatClassCompilationStatusChanged;

        public string AccountName { get; }

        public BattlegroundEngine BattlegroundEngine { get; set; }

        public IAmeisenBotCache BotCache { get; set; }

        public string BotDataPath { get; }

        public BotPersonality BotPersonality { get; set; }

        public CharacterManager CharacterManager { get; set; }

        public ICombatClass CombatClass { get; set; }

        public CombatLogParser CombatLogParser { get; set; }

        public AmeisenBotConfig Config { get; }

        public double CurrentExecutionMs
        {
            get
            {
                double avgTickTime = Math.Round(currentExecutionMs / CurrentExecutionCount, 2);
                CurrentExecutionCount = 0;
                return avgTickTime;
            }

            private set
            {
                currentExecutionMs = value;
            }
        }

        public EventHookManager EventHookManager { get; set; }

        public HookManager HookManager { get; set; }

        public bool IsRunning { get; private set; }

        public JobEngine JobEngine { get; set; }

        public IMovementEngine MovemenEngine { get; set; }

        public MovementSettings MovementSettings { get; set; }

        public ObjectManager ObjectManager { get; set; }

        public IOffsetList OffsetList { get; }

        public IPathfindingHandler PathfindingHandler { get; set; }

        public AmeisenBotStateMachine StateMachine { get; set; }

        public Process WowProcess { get; }

        private int CurrentExecutionCount { get; set; }

        private Timer StateMachineTimer { get; }

        private XMemory XMemory { get; }

        public void Pause()
        {
            AmeisenLogger.Instance.Log("AmeisenBot", "Pausing", LogLevel.Warning);
            IsRunning = false;
        }

        public void Resume()
        {
            AmeisenLogger.Instance.Log("AmeisenBot", "Resuming", LogLevel.Warning);
            IsRunning = true;
            stateMachineTimerBusy = 0;
        }

        public void Start()
        {
            AmeisenLogger.Instance.Log("AmeisenBot", "Starting", LogLevel.Warning);
            StateMachineTimer.Start();
            BotCache.Load();
            SubscribeToWowEvents();
            IsRunning = true;
        }

        public void Stop()
        {
            AmeisenLogger.Instance.Log("AmeisenBot", "Stopping", LogLevel.Warning);
            StateMachineTimer.Stop();

            HookManager.DisposeHook();
            EventHookManager.Stop();

            if (ObjectManager.Player?.Name.Length > 0)
            {
                if (Config.SaveWowWindowPosition)
                {
                    SaveWowWindowPosition();
                }

                if (Config.SaveBotWindowPosition)
                {
                    SaveBotWindowPosition();
                }
            }

            BotCache.Save();

            if (Config.AutocloseWow)
            {
                AmeisenLogger.Instance.Log("AmeisenBot", "Killing WoW process", LogLevel.Warning);
                XMemory.Process.Kill();
            }

            AmeisenLogger.Instance.Log("AmeisenBot", $"Stopping AmeisenBot...", LogLevel.Master);
            AmeisenLogger.Instance.Stop();
        }

        private static void SetupLogging(string botDataPath, string accountName)
        {
            AmeisenLogger.Instance.ChangeLogFolder(Path.Combine(botDataPath, accountName, "log/"));
            AmeisenLogger.Instance.ActiveLogLevel = LogLevel.Verbose;
            AmeisenLogger.Instance.Start();
        }

        private ICombatClass CompileCustomCombatClass()
        {
            CompilerParameters parameters = new CompilerParameters
            {
                GenerateInMemory = true,
                GenerateExecutable = false
            };

            foreach (string dependecy in Config.CustomCombatClassDependencies)
            {
                parameters.ReferencedAssemblies.Add(dependecy);
            }

            using CSharpCodeProvider codeProvider = new CSharpCodeProvider();
            CompilerResults results = codeProvider.CompileAssemblyFromSource(parameters, File.ReadAllText(Config.CustomCombatClassFile));

            if (results.Errors.HasErrors)
            {
                StringBuilder sb = new StringBuilder();

                foreach (CompilerError error in results.Errors)
                {
                    sb.AppendLine($"Error {error.ErrorNumber} Line: {error.Line}: {error.ErrorText}");
                }

                throw new InvalidOperationException(sb.ToString());
            }

            return (ICombatClass)results.CompiledAssembly.CreateInstance(typeof(ICombatClass).ToString());
        }

        private void HandleLoadWowPosition()
        {
            if (StateMachine.CurrentState.Key == BotState.Login)
            {
                if (Config.SaveWowWindowPosition)
                {
                    LoadWowWindowPosition();
                }

                if (Config.SaveBotWindowPosition)
                {
                    LoadBotWindowPosition();
                }
            }
        }

        private void LoadBotWindowPosition()
        {
            if (AccountName.Length > 0)
            {
                try
                {
                    if (Config.BotWindowRect != new Rect() { Left = -1, Top = -1, Right = -1, Bottom = -1 })
                    {
                        XMemory.SetWindowPosition(Process.GetCurrentProcess().MainWindowHandle, Config.BotWindowRect);
                        AmeisenLogger.Instance.Log("AmeisenBot", $"Loaded bot window position: {Config.BotWindowRect}", LogLevel.Verbose);
                    }
                }
                catch (Exception e)
                {
                    AmeisenLogger.Instance.Log("AmeisenBot", $"Failed to set bot window position:\n{e.ToString()}", LogLevel.Error);
                }
            }
        }

        private void LoadCustomCombatClass()
        {
            AmeisenLogger.Instance.Log("AmeisenBot", $"Loading custom CombatClass: {Config.CustomCombatClassFile}", LogLevel.Verbose);
            if (Config.CustomCombatClassFile.Length == 0
                || !File.Exists(Config.CustomCombatClassFile))
            {
                AmeisenLogger.Instance.Log("AmeisenBot", "Loading default CombatClass", LogLevel.Warning);
                LoadDefaultCombatClass();
            }
            else
            {
                try
                {
                    CombatClass = CompileCustomCombatClass();
                    OnCombatClassCompilationStatusChanged?.Invoke(true, string.Empty, string.Empty);
                    AmeisenLogger.Instance.Log("AmeisenBot", $"Compiling custom CombatClass successful", LogLevel.Warning);
                }
                catch (Exception e)
                {
                    AmeisenLogger.Instance.Log("AmeisenBot", $"Compiling custom CombatClass failed:\n{e}", LogLevel.Warning);
                    OnCombatClassCompilationStatusChanged?.Invoke(false, e.GetType().Name, e.ToString());
                    LoadDefaultCombatClass();
                }
            }
        }

        private void LoadDefaultCombatClass()
        {
            AmeisenLogger.Instance.Log("AmeisenBot", $"Loading built in CombatClass: {Config.BuiltInCombatClassName}", LogLevel.Verbose);
            CombatClass = (Config.BuiltInCombatClassName.ToUpper()) switch
            {
                "WARRIORARMS" => new WarriorArms(ObjectManager, CharacterManager, HookManager, PathfindingHandler, new DefaultMovementEngine(ObjectManager, MovementSettings)),
                "DEATHKNIGHTBLOOD" => new DeathknightBlood(ObjectManager, CharacterManager, HookManager),
                "DEATHKNIGHTUNHOLY" => new DeathknightUnholy(ObjectManager, CharacterManager, HookManager),
                "DEATHKNIGHTFROST" => new DeathknightFrost(ObjectManager, CharacterManager, HookManager),
                "WARRIORFURY" => new WarriorFury(ObjectManager, CharacterManager, HookManager, PathfindingHandler, new DefaultMovementEngine(ObjectManager, MovementSettings)),
                "PALADINHOLY" => new PaladinHoly(ObjectManager, CharacterManager, HookManager),
                "PALADINRETRIBUTION" => new PaladinRetribution(ObjectManager, CharacterManager, HookManager),
                "PALADINPROTECTION" => new PaladinProtection(ObjectManager, CharacterManager, HookManager, PathfindingHandler, new DefaultMovementEngine(ObjectManager, MovementSettings)),
                "MAGEARCANE" => new MageArcane(ObjectManager, CharacterManager, HookManager),
                "MAGEFIRE" => new MageFire(ObjectManager, CharacterManager, HookManager),
                "HUNTERBEASTMASTERY" => new HunterBeastmastery(ObjectManager, CharacterManager, HookManager),
                "HUNTERMARKSMANSHIP" => new HunterMarksmanship(ObjectManager, CharacterManager, HookManager),
                "HUNTERSURVIVAL" => new HunterSurvival(ObjectManager, CharacterManager, HookManager),
                "PRIESTHOLY" => new PriestHoly(ObjectManager, CharacterManager, HookManager),
                "PRIESTDISCIPLINE" => new PriestDiscipline(ObjectManager, CharacterManager, HookManager),
                "PRIESTSHADOW" => new PriestShadow(ObjectManager, CharacterManager, HookManager),
                "WARLOCKAFFLICTION" => new WarlockAffliction(ObjectManager, CharacterManager, HookManager),
                "WARLOCKDEMONOLOGY" => new WarlockDemonology(ObjectManager, CharacterManager, HookManager),
                "WARLOCKDESTRUCTION" => new WarlockDestruction(ObjectManager, CharacterManager, HookManager),
                "DRUIDRESTORATION" => new DruidRestoration(ObjectManager, CharacterManager, HookManager),
                "DRUIDBALANCE" => new DruidBalance(ObjectManager, CharacterManager, HookManager),
                "ROGUEASSASSINATION" => new RogueAssassination(ObjectManager, CharacterManager, HookManager),
                "ALTROGUEASSASSINATION" => new RogueAssassination2(ObjectManager, CharacterManager, HookManager, PathfindingHandler, new DefaultMovementEngine(ObjectManager, MovementSettings)),
                "SHAMANELEMENTAL" => new ShamanElemental(ObjectManager, CharacterManager, HookManager),
                "SHAMANRESTORATION" => new ShamanRestoration(ObjectManager, CharacterManager, HookManager),
                _ => null,
            };
        }

        private void LoadWowWindowPosition()
        {
            if (AccountName.Length > 0)
            {
                try
                {
                    if (Config.WowWindowRect != new Rect() { Left = -1, Top = -1, Right = -1, Bottom = -1 })
                    {
                        XMemory.SetWindowPosition(XMemory.Process.MainWindowHandle, Config.WowWindowRect);
                        AmeisenLogger.Instance.Log("AmeisenBot", $"Loaded wow window position: {Config.WowWindowRect}", LogLevel.Verbose);
                    }
                }
                catch (Exception e)
                {
                    AmeisenLogger.Instance.Log("AmeisenBot", $"Failed to set wow window position:\n{e.ToString()}", LogLevel.Error);
                }
            }
        }

        private void OnBagChanged(long timestamp, List<string> args)
        {
            AmeisenLogger.Instance.Log("WoWEvents", $"Event OnBagChanged: {JsonConvert.SerializeObject(args)}", LogLevel.Verbose);

            CharacterManager.Inventory.Update();
            CharacterManager.UpdateCharacterGear();
            CharacterManager.Inventory.Update();
        }

        private void OnBattlegroundScoreUpdate(long timestamp, List<string> args)
        {
            AmeisenLogger.Instance.Log("WoWEvents", $"Event OnBattlegroundScoreUpdate: {JsonConvert.SerializeObject(args)}", LogLevel.Verbose);
        }

        private void OnBgAllianceMessage(long timestamp, List<string> args)
        {
            AmeisenLogger.Instance.Log("WoWEvents", $"Event OnBgAllianceMessage: {JsonConvert.SerializeObject(args)}", LogLevel.Verbose);

            if (args.Count > 1)
            {
                ProcessBgMessage(args[0], args[1]);
            }
        }

        private void OnBgHordeMessage(long timestamp, List<string> args)
        {
            AmeisenLogger.Instance.Log("WoWEvents", $"Event OnBgHordeMessage: {JsonConvert.SerializeObject(args)}", LogLevel.Verbose);

            if (args.Count > 1)
            {
                ProcessBgMessage(args[0], args[1]);
            }
        }

        private void OnBgNeutralMessage(long timestamp, List<string> args)
        {
            AmeisenLogger.Instance.Log("WoWEvents", $"Event OnBgNeutralMessage: {JsonConvert.SerializeObject(args)}", LogLevel.Verbose);

            if (args.Count > 1)
            {
                ProcessBgMessage(args[0], args[1]);
            }
        }

        private void OnConfirmBindOnPickup(long timestamp, List<string> args)
        {
            AmeisenLogger.Instance.Log("WoWEvents", $"Event OnConfirmBindOnPickup: {JsonConvert.SerializeObject(args)}", LogLevel.Verbose);
            HookManager.CofirmBop();
        }

        private void OnEquipmentChanged(long timestamp, List<string> args)
        {
            AmeisenLogger.Instance.Log("WoWEvents", $"Event OnEquipmentChanged: {JsonConvert.SerializeObject(args)}", LogLevel.Verbose);
            CharacterManager.Equipment.Update();
        }

        private void OnLootRollStarted(long timestamp, List<string> args)
        {
            AmeisenLogger.Instance.Log("WoWEvents", $"Event OnLootRollStarted: {JsonConvert.SerializeObject(args)}", LogLevel.Verbose);

            if (int.TryParse(args[0], out int rollId))
            {
                string itemName = HookManager.GetLootRollItemLink(rollId);
                string json = HookManager.GetItemByNameOrLink(itemName);
                WowBasicItem item = ItemFactory.ParseItem(json);
                item = ItemFactory.BuildSpecificItem(item);

                if (CharacterManager.IsItemAnImprovement(item, out IWowItem itemToReplace))
                {
                    AmeisenLogger.Instance.Log("WoWEvents", $"Would like to replace item {item?.Name} with {itemToReplace?.Name}, rolling need", LogLevel.Verbose);
                    HookManager.RollOnItem(rollId, RollType.Need);
                    return;
                }
            }

            HookManager.RollOnItem(rollId, RollType.Pass);
        }

        private void OnLootWindowOpened(long timestamp, List<string> args)
        {
            AmeisenLogger.Instance.Log("WoWEvents", $"Event OnLootWindowOpened: {JsonConvert.SerializeObject(args)}", LogLevel.Verbose);
            HookManager.LootEveryThing();
        }

        private void OnPartyInvitation(long timestamp, List<string> args)
        {
            AmeisenLogger.Instance.Log("WoWEvents", $"Event OnPartyInvitation: {JsonConvert.SerializeObject(args)}", LogLevel.Verbose);
            HookManager.AcceptPartyInvite();
        }

        private void OnPvpQueueShow(long timestamp, List<string> args)
        {
            AmeisenLogger.Instance.Log("WoWEvents", $"Event OnPvpQueueShow: {JsonConvert.SerializeObject(args)}", LogLevel.Verbose);
        }

        private void OnReadyCheck(long timestamp, List<string> args)
        {
            AmeisenLogger.Instance.Log("WoWEvents", $"Event OnReadyCheck: {JsonConvert.SerializeObject(args)}", LogLevel.Verbose);
            HookManager.CofirmReadyCheck(true);
        }

        private void OnResurrectRequest(long timestamp, List<string> args)
        {
            AmeisenLogger.Instance.Log("WoWEvents", $"Event OnResurrectRequest: {JsonConvert.SerializeObject(args)}", LogLevel.Verbose);
            HookManager.AcceptResurrect();
        }

        private void OnSummonRequest(long timestamp, List<string> args)
        {
            AmeisenLogger.Instance.Log("WoWEvents", $"Event OnSummonRequest: {JsonConvert.SerializeObject(args)}", LogLevel.Verbose);
            HookManager.AcceptSummon();
        }

        private void OnWorldStateUpdate(long timestamp, List<string> args)
        {
            AmeisenLogger.Instance.Log("WoWEvents", $"Event OnWorldStateUpdate: {JsonConvert.SerializeObject(args)}", LogLevel.Verbose);
        }

        private void ProcessBgMessage(string message, string arg1)
        {
            if (message.ToUpper().Contains("ALLIANCE FLAG WAS PICKED UP"))
            {
                BattlegroundEngine.AllianceFlagWasPickedUp(arg1);
            }
            else if (message.ToUpper().Contains("HORDE FLAG WAS PICKED UP"))
            {
                BattlegroundEngine.HordeFlagWasPickedUp(arg1);
            }
            else if (message.ToUpper().Contains("ALLIANCE FLAG WAS DROPPED")
                || message.ToUpper().Contains("CAPTURED THE ALLIANCE FLAG")
                || message.ToUpper().Contains("THE ALLIANCE FLAG IS NOW PLACED AT ITS BASE"))
            {
                BattlegroundEngine.AllianceFlagWasDropped(arg1);
            }
            else if (message.ToUpper().Contains("HORDE FLAG WAS DROPPED")
                || message.ToUpper().Contains("CAPTURED THE HORDE FLAG")
                || message.ToUpper().Contains("THE HORDE FLAG IS NOW PLACED AT ITS BASE"))
            {
                BattlegroundEngine.HordeFlagWasDropped(arg1);
            }
        }

        private void SaveBotWindowPosition()
        {
            try
            {
                Config.BotWindowRect = XMemory.GetWindowPosition(Process.GetCurrentProcess().MainWindowHandle);
            }
            catch (Exception e)
            {
                AmeisenLogger.Instance.Log("AmeisenBot", $"Failed to save bot window position:\n{e.ToString()}", LogLevel.Error);
            }
        }

        private void SaveWowWindowPosition()
        {
            try
            {
                Config.WowWindowRect = XMemory.GetWindowPosition(XMemory.Process.MainWindowHandle);
            }
            catch (Exception e)
            {
                AmeisenLogger.Instance.Log("AmeisenBot", $"Failed to save wow window position:\n{e.ToString()}", LogLevel.Error);
            }
        }

        private void StateMachineTimerTick(object sender, ElapsedEventArgs e)
        {
            // only start one timer tick at a time
            if (Interlocked.CompareExchange(ref stateMachineTimerBusy, 1, 0) == 1
                || !IsRunning)
            {
                return;
            }

            try
            {
                Stopwatch watch = Stopwatch.StartNew();
                StateMachine.Execute();
                CurrentExecutionMs = watch.ElapsedMilliseconds;
                CurrentExecutionCount++;
            }
            finally
            {
                stateMachineTimerBusy = 0;
            }
        }

        private void SubscribeToWowEvents()
        {
            EventHookManager.Subscribe("PARTY_INVITE_REQUEST", OnPartyInvitation);
            EventHookManager.Subscribe("RESURRECT_REQUEST", OnResurrectRequest);
            EventHookManager.Subscribe("CONFIRM_SUMMON", OnSummonRequest);
            EventHookManager.Subscribe("READY_CHECK", OnReadyCheck);

            EventHookManager.Subscribe("LOOT_OPENED", OnLootWindowOpened);
            EventHookManager.Subscribe("LOOT_BIND_CONFIRM", OnConfirmBindOnPickup);
            EventHookManager.Subscribe("CONFIRM_LOOT_ROLL", OnConfirmBindOnPickup);
            EventHookManager.Subscribe("START_LOOT_ROLL", OnLootRollStarted);
            EventHookManager.Subscribe("BAG_UPDATE", OnBagChanged);
            EventHookManager.Subscribe("PLAYER_EQUIPMENT_CHANGED", OnEquipmentChanged);
            //// EventHookManager.Subscribe("DELETE_ITEM_CONFIRM", OnConfirmDeleteItem);

            EventHookManager.Subscribe("UPDATE_BATTLEFIELD_SCORE", OnBattlegroundScoreUpdate);
            EventHookManager.Subscribe("UPDATE_WORLD_STATES", OnWorldStateUpdate);
            EventHookManager.Subscribe("PVPQUEUE_ANYWHERE_SHOW", OnPvpQueueShow);
            EventHookManager.Subscribe("CHAT_MSG_BG_SYSTEM_ALLIANCE", OnBgAllianceMessage);
            EventHookManager.Subscribe("CHAT_MSG_BG_SYSTEM_HORDE", OnBgHordeMessage);
            EventHookManager.Subscribe("CHAT_MSG_BG_SYSTEM_NEUTRAL", OnBgNeutralMessage);

            // EventHookManager.Subscribe("COMBAT_LOG_EVENT_UNFILTERED", CombatLogParser.Parse);
        }
    }
}