using AmeisenBotX.Core.Autologin;
using AmeisenBotX.Core.Battleground;
using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Character.Inventory;
using AmeisenBotX.Core.Character.Inventory.Objects;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Cache;
using AmeisenBotX.Core.Data.CombatLog;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Dungeon;
using AmeisenBotX.Core.Event;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.Jobs;
using AmeisenBotX.Core.Movement;
using AmeisenBotX.Core.Movement.Settings;
using AmeisenBotX.Core.Offsets;
using AmeisenBotX.Core.Personality;
using AmeisenBotX.Core.Relaxing;
using AmeisenBotX.Core.Statemachine;
using AmeisenBotX.Core.Statemachine.CombatClasses;
using AmeisenBotX.Core.Statemachine.States;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using AmeisenBotX.Memory;
using AmeisenBotX.Memory.Win32;
using AmeisenBotX.Pathfinding;
using Microsoft.CSharp;
using Newtonsoft.Json;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
            string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            Config = config;
            AccountName = accountName;
            BotDataPath = botDataPath;

            CurrentExecutionMs = 0;
            CurrentExecutionCount = 0;
            stateMachineTimerBusy = 0;

            AmeisenLogger.Instance.Log("AmeisenBot", $"AmeisenBot ({version}) starting...", LogLevel.Master);
            AmeisenLogger.Instance.Log("AmeisenBot", $"AccountName: {accountName}", LogLevel.Master);
            AmeisenLogger.Instance.Log("AmeisenBot", $"BotDataPath: {botDataPath}", LogLevel.Verbose);

            Stopwatch = new Stopwatch();

            StateMachineTimer = new Timer(Config.StateMachineTickMs);
            StateMachineTimer.Elapsed += StateMachineTimerTick;

            WowInterface = new WowInterface();
            SetupWowInterface();

            AmeisenLogger.Instance.Log("AmeisenBot", $"Using OffsetList: {WowInterface.OffsetList.GetType()}", LogLevel.Master);

            if (!Directory.Exists(BotDataPath))
            {
                Directory.CreateDirectory(BotDataPath);
                AmeisenLogger.Instance.Log("AmeisenBot", $"Creating folder {botDataPath}", LogLevel.Verbose);
            }

            InitCombatClasses();
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
            if (WowInterface.CombatClass?.ItemComparator != null)
            {
                WowInterface.CharacterManager.ItemComparator = WowInterface.CombatClass.ItemComparator;
            }

            StateMachine = new AmeisenBotStateMachine(BotDataPath, Config, WowInterface);
            StateMachine.OnStateMachineStateChanged += HandleLoadWowPosition;
        }

        public delegate void CombatClassCompilationStatus(bool succeeded, string heading, string message);

        public event CombatClassCompilationStatus OnCombatClassCompilationStatusChanged;

        public string AccountName { get; }

        public string BotDataPath { get; }

        public List<ICombatClass> CombatClasses { get; private set; }

        public AmeisenBotConfig Config { get; set; }

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

        public bool IsAutopilot { get; set; }

        public bool IsRunning { get; private set; }

        public AmeisenBotStateMachine StateMachine { get; set; }

        public Stopwatch Stopwatch { get; private set; }

        public WowInterface WowInterface { get; set; }

        private int CurrentExecutionCount { get; set; }

        private Timer StateMachineTimer { get; }

        public void Pause()
        {
            AmeisenLogger.Instance.Log("AmeisenBot", "Pausing", LogLevel.Warning);
            IsRunning = false;
        }

        public void ReloadConfig()
        {
            StateMachineTimer.Interval = Config.StateMachineTickMs;

            if (Config.UseBuiltInCombatClass)
            {
                LoadDefaultCombatClass();
            }
            else
            {
                LoadCustomCombatClass();
            }
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
            WowInterface.BotCache.Load();
            SubscribeToWowEvents();
            IsRunning = true;
        }

        public void Stop()
        {
            AmeisenLogger.Instance.Log("AmeisenBot", "Stopping", LogLevel.Warning);
            StateMachineTimer.Stop();

            WowInterface.HookManager.DisposeHook();
            WowInterface.EventHookManager.Stop();

            if (WowInterface.ObjectManager.Player?.Name?.Length > 0)
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

            WowInterface.BotCache.Save();

            if (Config.AutocloseWow)
            {
                AmeisenLogger.Instance.Log("AmeisenBot", "Killing WoW process", LogLevel.Warning);
                WowInterface.XMemory.Process.Kill();
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

        private void InitCombatClasses()
        {
            CombatClasses = new List<ICombatClass>
            {
                new Statemachine.CombatClasses.Jannis.DeathknightFrost(WowInterface),
                new Statemachine.CombatClasses.Jannis.DeathknightUnholy(WowInterface),
                new Statemachine.CombatClasses.Jannis.DruidBalance(WowInterface),
                new Statemachine.CombatClasses.Jannis.DruidRestoration(WowInterface),
                new Statemachine.CombatClasses.Jannis.HunterBeastmastery(WowInterface),
                new Statemachine.CombatClasses.Jannis.HunterMarksmanship(WowInterface),
                new Statemachine.CombatClasses.Jannis.HunterSurvival(WowInterface),
                new Statemachine.CombatClasses.Jannis.MageArcane(WowInterface),
                new Statemachine.CombatClasses.Jannis.MageFire(WowInterface),
                new Statemachine.CombatClasses.Jannis.PaladinHoly(WowInterface),
                new Statemachine.CombatClasses.Jannis.PaladinRetribution(WowInterface),
                new Statemachine.CombatClasses.Jannis.PriestDiscipline(WowInterface),
                new Statemachine.CombatClasses.Jannis.PriestHoly(WowInterface),
                new Statemachine.CombatClasses.Jannis.PriestShadow(WowInterface),
                new Statemachine.CombatClasses.Jannis.RogueAssassination(WowInterface),
                new Statemachine.CombatClasses.Jannis.ShamanElemental(WowInterface),
                new Statemachine.CombatClasses.Jannis.ShamanRestoration(WowInterface),
                new Statemachine.CombatClasses.Jannis.WarlockAffliction(WowInterface),
                new Statemachine.CombatClasses.Jannis.WarlockDemonology(WowInterface),
                new Statemachine.CombatClasses.Jannis.WarlockDestruction(WowInterface),
                new Statemachine.CombatClasses.Jannis.WarriorArms(WowInterface),
                new Statemachine.CombatClasses.Jannis.WarriorFury(WowInterface),
                new Statemachine.CombatClasses.Jannis.WarriorProtection(WowInterface),
                new PaladinProtection(WowInterface.ObjectManager, WowInterface.CharacterManager, WowInterface.HookManager, WowInterface.PathfindingHandler, new DefaultMovementEngine(WowInterface.ObjectManager, WowInterface.MovementSettings)),
                new WarriorArms(WowInterface.ObjectManager, WowInterface.CharacterManager, WowInterface.HookManager, WowInterface.PathfindingHandler, new DefaultMovementEngine(WowInterface.ObjectManager, WowInterface.MovementSettings)),
                new WarriorFury(WowInterface.ObjectManager, WowInterface.CharacterManager, WowInterface.HookManager, WowInterface.PathfindingHandler, new DefaultMovementEngine(WowInterface.ObjectManager, WowInterface.MovementSettings)),
                new RogueAssassination2(WowInterface.ObjectManager, WowInterface.CharacterManager, WowInterface.HookManager, WowInterface.PathfindingHandler, new DefaultMovementEngine(WowInterface.ObjectManager, WowInterface.MovementSettings)),
                new DeathknightBlood(WowInterface.ObjectManager, WowInterface.CharacterManager, WowInterface.HookManager),
            };
        }

        private void LoadBotWindowPosition()
        {
            if (AccountName.Length > 0)
            {
                try
                {
                    if (Config.BotWindowRect != new Rect() { Left = -1, Top = -1, Right = -1, Bottom = -1 })
                    {
                        XMemory.SetWindowPosition(Process.GetCurrentProcess().MainWindowHandle, Config.BotWindowRect, false);
                        AmeisenLogger.Instance.Log("AmeisenBot", $"Loaded bot window position: {Config.BotWindowRect}", LogLevel.Verbose);
                    }
                }
                catch (Exception e)
                {
                    AmeisenLogger.Instance.Log("AmeisenBot", $"Failed to set bot window position:\n{e}", LogLevel.Error);
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
                    WowInterface.CombatClass = CompileCustomCombatClass();
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
            WowInterface.CombatClass = CombatClasses.FirstOrDefault(e => e.ToString().Equals(Config.BuiltInCombatClassName, StringComparison.OrdinalIgnoreCase));
        }

        private void LoadWowWindowPosition()
        {
            if (AccountName.Length > 0)
            {
                try
                {
                    if (Config.WowWindowRect != new Rect() { Left = -1, Top = -1, Right = -1, Bottom = -1 })
                    {
                        XMemory.SetWindowPosition(WowInterface.XMemory.Process.MainWindowHandle, Config.WowWindowRect);
                        AmeisenLogger.Instance.Log("AmeisenBot", $"Loaded wow window position: {Config.WowWindowRect}", LogLevel.Verbose);
                    }
                }
                catch (Exception e)
                {
                    AmeisenLogger.Instance.Log("AmeisenBot", $"Failed to set wow window position:\n{e}", LogLevel.Error);
                }
            }
        }

        private void OnBagChanged(long timestamp, List<string> args)
        {
            AmeisenLogger.Instance.Log("WoWEvents", $"Event OnBagChanged: {JsonConvert.SerializeObject(args)}", LogLevel.Verbose);

            WowInterface.CharacterManager.Inventory.Update();
            WowInterface.CharacterManager.UpdateCharacterGear();
            WowInterface.CharacterManager.Inventory.Update();
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
            WowInterface.HookManager.CofirmBop();
        }

        private void OnEquipmentChanged(long timestamp, List<string> args)
        {
            AmeisenLogger.Instance.Log("WoWEvents", $"Event OnEquipmentChanged: {JsonConvert.SerializeObject(args)}", LogLevel.Verbose);
            WowInterface.CharacterManager.Equipment.Update();
        }

        private void OnLfgProposalShow(long timestamp, List<string> args)
        {
            WowInterface.HookManager.SendChatMessage("/click LFDDungeonReadyDialogEnterDungeonButton");
        }

        private void OnLfgRoleCheckShow(long timestamp, List<string> args)
        {
            string selectRoleString = WowInterface.CombatClass != null ? WowInterface.CombatClass.Role switch
            {
                Statemachine.Enums.CombatClassRole.Tank => "/click LFDRoleCheckPopupRoleButtonTank",
                Statemachine.Enums.CombatClassRole.Heal => "/click LFDRoleCheckPopupRoleButtonHealer",
                Statemachine.Enums.CombatClassRole.Dps => "/click LFDRoleCheckPopupRoleButtonDPS",
                _ => "/click LFDRoleCheckPopupRoleButtonDPS",
            } : "/click LFDRoleCheckPopupRoleButtonDPS";

            // do this twice to ensure that we join the queue
            WowInterface.HookManager.SendChatMessage(selectRoleString);
            WowInterface.HookManager.SendChatMessage("/click LFDRoleCheckPopupAcceptButton");

            WowInterface.HookManager.SendChatMessage(selectRoleString);
            WowInterface.HookManager.SendChatMessage("/click LFDRoleCheckPopupAcceptButton");
        }

        private void OnLootRollStarted(long timestamp, List<string> args)
        {
            AmeisenLogger.Instance.Log("WoWEvents", $"Event OnLootRollStarted: {JsonConvert.SerializeObject(args)}", LogLevel.Verbose);

            if (int.TryParse(args[0], out int rollId))
            {
                string itemName = WowInterface.HookManager.GetLootRollItemLink(rollId);
                string json = WowInterface.HookManager.GetItemByNameOrLink(itemName);
                WowBasicItem item = ItemFactory.ParseItem(json);
                item = ItemFactory.BuildSpecificItem(item);

                if (WowInterface.CharacterManager.IsItemAnImprovement(item, out IWowItem itemToReplace))
                {
                    AmeisenLogger.Instance.Log("WoWEvents", $"Would like to replace item {item?.Name} with {itemToReplace?.Name}, rolling need", LogLevel.Verbose);
                    WowInterface.HookManager.RollOnItem(rollId, RollType.Need);
                    return;
                }
            }

            WowInterface.HookManager.RollOnItem(rollId, RollType.Pass);
        }

        private void OnLootWindowOpened(long timestamp, List<string> args)
        {
            AmeisenLogger.Instance.Log("WoWEvents", $"Event OnLootWindowOpened: {JsonConvert.SerializeObject(args)}", LogLevel.Verbose);
            WowInterface.HookManager.LootEveryThing();
        }

        private void OnPartyInvitation(long timestamp, List<string> args)
        {
            AmeisenLogger.Instance.Log("WoWEvents", $"Event OnPartyInvitation: {JsonConvert.SerializeObject(args)}", LogLevel.Verbose);
            WowInterface.HookManager.AcceptPartyInvite();
        }

        private void OnPvpQueueShow(long timestamp, List<string> args)
        {
            AmeisenLogger.Instance.Log("WoWEvents", $"Event OnPvpQueueShow: {JsonConvert.SerializeObject(args)}", LogLevel.Verbose);
        }

        private void OnReadyCheck(long timestamp, List<string> args)
        {
            AmeisenLogger.Instance.Log("WoWEvents", $"Event OnReadyCheck: {JsonConvert.SerializeObject(args)}", LogLevel.Verbose);
            WowInterface.HookManager.CofirmReadyCheck(true);
        }

        private void OnResurrectRequest(long timestamp, List<string> args)
        {
            AmeisenLogger.Instance.Log("WoWEvents", $"Event OnResurrectRequest: {JsonConvert.SerializeObject(args)}", LogLevel.Verbose);
            WowInterface.HookManager.AcceptResurrect();
        }

        private void OnSummonRequest(long timestamp, List<string> args)
        {
            AmeisenLogger.Instance.Log("WoWEvents", $"Event OnSummonRequest: {JsonConvert.SerializeObject(args)}", LogLevel.Verbose);
            WowInterface.HookManager.AcceptSummon();
        }

        private void OnWorldStateUpdate(long timestamp, List<string> args)
        {
            AmeisenLogger.Instance.Log("WoWEvents", $"Event OnWorldStateUpdate: {JsonConvert.SerializeObject(args)}", LogLevel.Verbose);
        }

        private void ProcessBgMessage(string message, string arg1)
        {
            if (message.ToUpper().Contains("ALLIANCE FLAG WAS PICKED UP"))
            {
                WowInterface.BattlegroundEngine.AllianceFlagWasPickedUp(arg1);
            }
            else if (message.ToUpper().Contains("HORDE FLAG WAS PICKED UP"))
            {
                WowInterface.BattlegroundEngine.HordeFlagWasPickedUp(arg1);
            }
            else if (message.ToUpper().Contains("ALLIANCE FLAG WAS DROPPED")
                || message.ToUpper().Contains("CAPTURED THE ALLIANCE FLAG")
                || message.ToUpper().Contains("THE ALLIANCE FLAG IS NOW PLACED AT ITS BASE"))
            {
                WowInterface.BattlegroundEngine.AllianceFlagWasDropped(arg1);
            }
            else if (message.ToUpper().Contains("HORDE FLAG WAS DROPPED")
                || message.ToUpper().Contains("CAPTURED THE HORDE FLAG")
                || message.ToUpper().Contains("THE HORDE FLAG IS NOW PLACED AT ITS BASE"))
            {
                WowInterface.BattlegroundEngine.HordeFlagWasDropped(arg1);
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
                AmeisenLogger.Instance.Log("AmeisenBot", $"Failed to save bot window position:\n{e}", LogLevel.Error);
            }
        }

        private void SaveWowWindowPosition()
        {
            try
            {
                Config.WowWindowRect = XMemory.GetWindowPosition(WowInterface.XMemory.Process.MainWindowHandle);
            }
            catch (Exception e)
            {
                AmeisenLogger.Instance.Log("AmeisenBot", $"Failed to save wow window position:\n{e}", LogLevel.Error);
            }
        }

        private void SetupWowInterface()
        {
            WowInterface.OffsetList = new OffsetList335a();
            WowInterface.XMemory = new XMemory();

            WowInterface.LoginHandler = new DefaultLoginHandler(WowInterface);

            WowInterface.BotCache = new InMemoryBotCache(Path.Combine(BotDataPath, AccountName, "cache.bin"));
            WowInterface.BotPersonality = new BotPersonality(Path.Combine(BotDataPath, AccountName, "personality.bin"));

            WowInterface.CombatLogParser = new CombatLogParser(WowInterface);

            WowInterface.ObjectManager = new ObjectManager(WowInterface);
            WowInterface.HookManager = new HookManager(WowInterface);
            WowInterface.CharacterManager = new CharacterManager(Config, WowInterface);
            WowInterface.EventHookManager = new EventHook(WowInterface);

            WowInterface.BattlegroundEngine = new BattlegroundEngine(WowInterface);
            WowInterface.JobEngine = new JobEngine(WowInterface);
            WowInterface.DungeonEngine = new DungeonEngine(WowInterface, StateMachine);
            WowInterface.RelaxEngine = new RelaxEngine(WowInterface);

            WowInterface.PathfindingHandler = new NavmeshServerClient(Config.NavmeshServerIp, Config.NameshServerPort);
            WowInterface.MovementSettings = new MovementSettings();
            WowInterface.MovementEngine = new SmartMovementEngine(WowInterface, WowInterface.MovementSettings);
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
                Stopwatch.Restart();
                StateMachine.Execute();
                CurrentExecutionMs = Stopwatch.ElapsedMilliseconds;
                CurrentExecutionCount++;
            }
            finally
            {
                stateMachineTimerBusy = 0;
            }
        }

        private void SubscribeToWowEvents()
        {
            WowInterface.EventHookManager.Subscribe("PARTY_INVITE_REQUEST", OnPartyInvitation);
            WowInterface.EventHookManager.Subscribe("RESURRECT_REQUEST", OnResurrectRequest);
            WowInterface.EventHookManager.Subscribe("CONFIRM_SUMMON", OnSummonRequest);
            WowInterface.EventHookManager.Subscribe("READY_CHECK", OnReadyCheck);

            WowInterface.EventHookManager.Subscribe("LOOT_OPENED", OnLootWindowOpened);
            WowInterface.EventHookManager.Subscribe("LOOT_BIND_CONFIRM", OnConfirmBindOnPickup);
            WowInterface.EventHookManager.Subscribe("CONFIRM_LOOT_ROLL", OnConfirmBindOnPickup);
            WowInterface.EventHookManager.Subscribe("START_LOOT_ROLL", OnLootRollStarted);
            WowInterface.EventHookManager.Subscribe("BAG_UPDATE", OnBagChanged);
            WowInterface.EventHookManager.Subscribe("PLAYER_EQUIPMENT_CHANGED", OnEquipmentChanged);

            // WowInterface.EventHookManager.Subscribe("DELETE_ITEM_CONFIRM", OnConfirmDeleteItem);

            WowInterface.EventHookManager.Subscribe("UPDATE_BATTLEFIELD_SCORE", OnBattlegroundScoreUpdate);
            WowInterface.EventHookManager.Subscribe("UPDATE_WORLD_STATES", OnWorldStateUpdate);
            WowInterface.EventHookManager.Subscribe("PVPQUEUE_ANYWHERE_SHOW", OnPvpQueueShow);
            WowInterface.EventHookManager.Subscribe("CHAT_MSG_BG_SYSTEM_ALLIANCE", OnBgAllianceMessage);
            WowInterface.EventHookManager.Subscribe("CHAT_MSG_BG_SYSTEM_HORDE", OnBgHordeMessage);
            WowInterface.EventHookManager.Subscribe("CHAT_MSG_BG_SYSTEM_NEUTRAL", OnBgNeutralMessage);

            WowInterface.EventHookManager.Subscribe("LFG_ROLE_CHECK_SHOW", OnLfgRoleCheckShow);
            WowInterface.EventHookManager.Subscribe("LFG_PROPOSAL_SHOW", OnLfgProposalShow);

            // WowInterface.EventHookManager.Subscribe("COMBAT_LOG_EVENT_UNFILTERED", WowInterface.CombatLogParser.Parse);
        }
    }
}