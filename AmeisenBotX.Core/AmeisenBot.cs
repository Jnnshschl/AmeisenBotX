using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Character.Inventory;
using AmeisenBotX.Core.Character.Inventory.Objects;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Persistence;
using AmeisenBotX.Core.Event;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.Movement;
using AmeisenBotX.Core.Movement.Settings;
using AmeisenBotX.Core.OffsetLists;
using AmeisenBotX.Core.StateMachine;
using AmeisenBotX.Core.StateMachine.CombatClasses;
using AmeisenBotX.Core.StateMachine.States;
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
            AmeisenLogger.Instance.Log("AmeisenBot starting...", LogLevel.Master);

            string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            AmeisenLogger.Instance.Log($"version: {version}", LogLevel.Master);

            Config = config;

            AccountName = accountName;
            AmeisenLogger.Instance.Log($"AccountName: {botDataPath}", LogLevel.Master);

            BotDataPath = botDataPath;
            AmeisenLogger.Instance.Log($"BotDataPath: {botDataPath}", LogLevel.Verbose);

            CurrentExecutionMs = 0;
            CurrentExecutionCount = 0;
            stateMachineTimerBusy = 0;

            StateMachineTimer = new Timer(config.StateMachineTickMs);
            StateMachineTimer.Elapsed += StateMachineTimerTick;

            OffsetList = new OffsetList335a();
            AmeisenLogger.Instance.Log($"Using OffsetList: {OffsetList.GetType().ToString()}", LogLevel.Master);

            XMemory = new XMemory();
            BotCache = new InMemoryBotCache(Path.Combine(BotDataPath, accountName, "cache.bin"));
            ObjectManager = new ObjectManager(XMemory, OffsetList, BotCache);
            HookManager = new HookManager(XMemory, OffsetList, ObjectManager, BotCache);
            CharacterManager = new CharacterManager(XMemory, config, OffsetList, ObjectManager, HookManager);
            EventHookManager = new EventHookManager(HookManager);
            PathfindingHandler = new NavmeshServerClient(Config.NavmeshServerIp, Config.NameshServerPort);
            MovementSettings = new MovementSettings();
            MovemenEngine = new DefaultMovementEngine(ObjectManager, MovementSettings);

            if (!Directory.Exists(BotDataPath))
            {
                Directory.CreateDirectory(BotDataPath);
                AmeisenLogger.Instance.Log($"Creating folder {botDataPath}", LogLevel.Verbose);
            }

            if (config.UseBuiltInCombatClass)
            {
                LoadDefaultCombatClass();
            }
            else
            {
                LoadCustomCombatClass();
            }

            StateMachine = new AmeisenBotStateMachine(BotDataPath, WowProcess, Config, XMemory, OffsetList, ObjectManager, CharacterManager, HookManager, EventHookManager, BotCache, PathfindingHandler, MovemenEngine, CombatClass);
            StateMachine.OnStateMachineStateChanged += HandlePositionLoad;
        }

        private void LoadCustomCombatClass()
        {
            AmeisenLogger.Instance.Log($"Loading custom CombatClass: {Config.CustomCombatClassFile}", LogLevel.Verbose);
            if (Config.CustomCombatClassFile.Length == 0
                || !File.Exists(Config.CustomCombatClassFile))
            {
                LoadDefaultCombatClass();
            }
            else
            {
                try
                {
                    CombatClass = CompileCustomCombatClass();
                    OnCombatClassCompilationStatusChanged?.Invoke(true, string.Empty, string.Empty);
                }
                catch (Exception e)
                {
                    OnCombatClassCompilationStatusChanged?.Invoke(false, e.GetType().Name, e.ToString());
                    LoadDefaultCombatClass();
                }
            }
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

        private void LoadDefaultCombatClass()
        {
            AmeisenLogger.Instance.Log($"Loading built in CombatClass: {Config.BuiltInCombatClassName}", LogLevel.Verbose);
            CombatClass = (Config.BuiltInCombatClassName.ToUpper()) switch
            {
                "WARRIORARMS" => new WarriorArms(ObjectManager, CharacterManager, HookManager, PathfindingHandler, MovemenEngine),
                "DEATHKNIGHTBLOOD" => new DeathknightBlood(ObjectManager, CharacterManager, HookManager),
                "DEATHKNIGHTUNHOLY" => new DeathknightUnholy(ObjectManager, CharacterManager, HookManager),
                "WARRIORFURY" => new WarriorFury(ObjectManager, CharacterManager, HookManager, PathfindingHandler, MovemenEngine),
                "PALADINHOLY" => new PaladinHoly(ObjectManager, CharacterManager, HookManager),
                "PALADINRETRIBUTION" => new PaladinRetribution(ObjectManager, CharacterManager, HookManager),
                "PALADINPROTECTION" => new PaladinProtection(ObjectManager, CharacterManager, HookManager, PathfindingHandler, MovemenEngine),
                "MAGEARCANE" => new MageArcane(ObjectManager, CharacterManager, HookManager),
                "MAGEFIRE" => new MageFire(ObjectManager, CharacterManager, HookManager),
                "HUNTERBEASTMASTERY" => new HunterBeastmastery(ObjectManager, CharacterManager, HookManager, XMemory),
                "PRIESTHOLY" => new PriestHoly(ObjectManager, CharacterManager, HookManager),
                "PRIESTSHADOW" => new PriestShadow(ObjectManager, CharacterManager, HookManager),
                "WARLOCKAFFLICTION" => new WarlockAffliction(ObjectManager, CharacterManager, HookManager),
                "DRUIDRESTORATION" => new DruidRestoration(ObjectManager, CharacterManager, HookManager),
                "DRUIDBALANCE" => new DruidBalance(ObjectManager, CharacterManager, HookManager),
                "ROGUEASSASSINATION" => new RogueAssassination(ObjectManager, CharacterManager, HookManager),
                "SHAMANELEMENTAL" => new ShamanElemental(ObjectManager, CharacterManager, HookManager),                
                _ => null,
            };
        }

        public delegate void CombatClassCompilationStatus(bool succeeded, string heading, string message);

        public event CombatClassCompilationStatus OnCombatClassCompilationStatusChanged;

        public string AccountName { get; }

        public IAmeisenBotCache BotCache { get; set; }

        public string BotDataPath { get; }

        public CharacterManager CharacterManager { get; set; }

        public ICombatClass CombatClass { get; set; }

        public AmeisenBotConfig Config { get; }

        public MovementSettings MovementSettings { get; set; }

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

        public DefaultMovementEngine MovemenEngine { get; set; }

        public ObjectManager ObjectManager { get; set; }

        public IOffsetList OffsetList { get; }

        public IPathfindingHandler PathfindingHandler { get; set; }

        public AmeisenBotStateMachine StateMachine { get; set; }

        public Process WowProcess { get; }

        private int CurrentExecutionCount { get; set; }

        private Timer StateMachineTimer { get; }

        private XMemory XMemory { get; }

        public bool IsRunning { get; private set; }

        public void Start()
        {
            StateMachineTimer.Start();
            BotCache.Load();
            SubscribeToWowEvents();
            IsRunning = true;
        }

        public void Stop()
        {
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
                XMemory.Process.Kill();
            }

            AmeisenLogger.Instance.Log($"Stopping AmeisenBot...", LogLevel.Master);
            AmeisenLogger.Instance.Stop();
        }

        private static void SetupLogging(string botDataPath, string accountName)
        {
            AmeisenLogger.Instance.ChangeLogFolder(Path.Combine(botDataPath, accountName, "log/"));
            AmeisenLogger.Instance.ActiveLogLevel = LogLevel.Verbose;
            AmeisenLogger.Instance.Start();
        }

        private void HandlePositionLoad()
        {
            if (StateMachine.CurrentState.Key == AmeisenBotState.Login)
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
                        AmeisenLogger.Instance.Log($"Loaded bot window position: {Config.BotWindowRect}", LogLevel.Verbose);
                    }
                }
                catch (Exception e)
                {
                    AmeisenLogger.Instance.Log($"Failed to set bot window position:\n{e.ToString()}", LogLevel.Error);
                }
            }
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
                        AmeisenLogger.Instance.Log($"Loaded wow window position: {Config.WowWindowRect}", LogLevel.Verbose);
                    }
                }
                catch (Exception e)
                {
                    AmeisenLogger.Instance.Log($"Failed to set wow window position:\n{e.ToString()}", LogLevel.Error);
                }
            }
        }

        private void OnBagChanged(long timestamp, List<string> args)
        {
            AmeisenLogger.Instance.Log($"Event OnBagChanged: {JsonConvert.SerializeObject(args)}", LogLevel.Verbose);

            CharacterManager.Inventory.Update();
            CharacterManager.UpdateCharacterGear();
            CharacterManager.Equipment.Update();
        }

        private void OnCombatLog(long timestamp, List<string> args)
        {
            // analyze the combat log
        }

        private void OnConfirmBindOnPickup(long timestamp, List<string> args)
        {
            AmeisenLogger.Instance.Log($"Event OnConfirmBindOnPickup: {JsonConvert.SerializeObject(args)}", LogLevel.Verbose);
            HookManager.CofirmBop();
        }

        private void OnEquipmentChanged(long timestamp, List<string> args)
        {
            AmeisenLogger.Instance.Log($"Event OnEquipmentChanged: {JsonConvert.SerializeObject(args)}", LogLevel.Verbose);
            CharacterManager.Equipment.Update();
        }

        private void OnLootRollStarted(long timestamp, List<string> args)
        {
            AmeisenLogger.Instance.Log($"Event OnLootRollStarted: {JsonConvert.SerializeObject(args)}", LogLevel.Verbose);

            if (int.TryParse(args[0], out int rollId))
            {
                string itemName = HookManager.GetLootRollItemLink(rollId);
                string json = HookManager.GetItemByNameOrLink(itemName);
                WowBasicItem item = ItemFactory.ParseItem(json);
                item = ItemFactory.BuildSpecificItem(item);

                if (CharacterManager.IsItemAnImprovement(item, out IWowItem itemToReplace))
                {
                    AmeisenLogger.Instance.Log($"Would like to replace item {item?.Name} with {itemToReplace?.Name}, rolling need", LogLevel.Verbose);
                    HookManager.RollOnItem(rollId, RollType.Need);
                    return;
                }
            }

            HookManager.RollOnItem(rollId, RollType.Pass);
        }

        private void OnLootWindowOpened(long timestamp, List<string> args)
        {
            AmeisenLogger.Instance.Log($"Event OnLootWindowOpened: {JsonConvert.SerializeObject(args)}", LogLevel.Verbose);
            HookManager.LootEveryThing();
        }

        private void OnPartyInvitation(long timestamp, List<string> args)
        {
            AmeisenLogger.Instance.Log($"Event OnPartyInvitation: {JsonConvert.SerializeObject(args)}", LogLevel.Verbose);
            HookManager.AcceptPartyInvite();
        }

        private void OnReadyCheck(long timestamp, List<string> args)
        {
            AmeisenLogger.Instance.Log($"Event OnReadyCheck: {JsonConvert.SerializeObject(args)}", LogLevel.Verbose);
            HookManager.CofirmReadyCheck(true);
        }

        public void Pause()
        {
            IsRunning = false;
        }

        private void OnResurrectRequest(long timestamp, List<string> args)
        {
            AmeisenLogger.Instance.Log($"Event OnResurrectRequest: {JsonConvert.SerializeObject(args)}", LogLevel.Verbose);
            HookManager.AcceptResurrect();
        }

        public void Resume()
        {
            IsRunning = true;
            stateMachineTimerBusy = 0;
        }

        private void OnSummonRequest(long timestamp, List<string> args)
        {
            AmeisenLogger.Instance.Log($"Event OnSummonRequest: {JsonConvert.SerializeObject(args)}", LogLevel.Verbose);
            HookManager.AcceptSummon();
        }

        private void SaveBotWindowPosition()
        {
            try
            {
                Config.BotWindowRect = XMemory.GetWindowPosition(Process.GetCurrentProcess().MainWindowHandle);
            }
            catch (Exception e)
            {
                AmeisenLogger.Instance.Log($"Failed to save bot window position:\n{e.ToString()}", LogLevel.Error);
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
                AmeisenLogger.Instance.Log($"Failed to save wow window position:\n{e.ToString()}", LogLevel.Error);
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
            //// EventHookManager.Subscribe("COMBAT_LOG_EVENT_UNFILTERED", OnCombatLog);
        }
    }
}
