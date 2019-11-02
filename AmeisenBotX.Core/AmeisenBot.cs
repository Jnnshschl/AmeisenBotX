using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Character.Inventory;
using AmeisenBotX.Core.Character.Inventory.Objects;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Persistence;
using AmeisenBotX.Core.Event;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.Movement;
using AmeisenBotX.Core.OffsetLists;
using AmeisenBotX.Core.StateMachine;
using AmeisenBotX.Core.StateMachine.CombatClasses;
using AmeisenBotX.Core.StateMachine.States;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using AmeisenBotX.Memory;
using AmeisenBotX.Memory.Win32;
using AmeisenBotX.Pathfinding;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
            AmeisenLogger.Instance.ChangeLogFolder(Path.Combine(botDataPath, accountName, "log/"));
            AmeisenLogger.Instance.ActiveLogLevel = LogLevel.Verbose;
            AmeisenLogger.Instance.Start();
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
            MovemenEngine = new DefaultMovementEngine();

            if (!Directory.Exists(BotDataPath))
            {
                Directory.CreateDirectory(BotDataPath);
                AmeisenLogger.Instance.Log($"Creating folder {botDataPath}", LogLevel.Verbose);
            }

            AmeisenLogger.Instance.Log($"Loading CombatClass: {Config.CombatClassName}", LogLevel.Verbose);
            CombatClass = (Config.CombatClassName.ToUpper()) switch
            {
                "WARRIORARMS" => new WarriorArms(ObjectManager, CharacterManager, HookManager),
                "DEATHKNIGHTBLOOD" => new DeathKnightBlood(ObjectManager, CharacterManager, HookManager),
                "WARRIORFURY" => new WarriorFury(ObjectManager, CharacterManager, HookManager, PathfindingHandler, MovemenEngine),
                "PALADINHOLY" => new PaladinHoly(ObjectManager, CharacterManager, HookManager),
                "MAGEFIRE" => new MageFire(ObjectManager, CharacterManager, HookManager),
                _ => null,
            };

            StateMachine = new AmeisenBotStateMachine(BotDataPath, WowProcess, Config, XMemory, OffsetList, ObjectManager, CharacterManager, HookManager, EventHookManager, BotCache, PathfindingHandler, MovemenEngine, CombatClass);
            StateMachine.OnStateMachineStateChange += HandlePositionLoad;
        }

        public string BotDataPath { get; }

        public string AccountName { get; }

        public IAmeisenBotCache BotCache { get; set; }

        public CharacterManager CharacterManager { get; set; }

        public ICombatClass CombatClass { get; set; }

        public AmeisenBotConfig Config { get; }

        public EventHookManager EventHookManager { get; set; }

        public HookManager HookManager { get; set; }

        public ObjectManager ObjectManager { get; set; }

        public IOffsetList OffsetList { get; }

        public IPathfindingHandler PathfindingHandler { get; set; }

        public DefaultMovementEngine MovemenEngine { get; set; }

        public AmeisenBotStateMachine StateMachine { get; set; }

        public Process WowProcess { get; }

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

        private int CurrentExecutionCount { get; set; }

        private Timer StateMachineTimer { get; }

        private XMemory XMemory { get; }

        public void Start()
        {
            StateMachineTimer.Start();
            BotCache.Load();

            EventHookManager.Subscribe("PARTY_INVITE_REQUEST", OnPartyInvitation);
            EventHookManager.Subscribe("RESURRECT_REQUEST", OnResurrectRequest);
            EventHookManager.Subscribe("CONFIRM_SUMMON", OnSummonRequest);
            EventHookManager.Subscribe("READY_CHECK", OnReadyCheck);
            EventHookManager.Subscribe("LOOT_OPENED", OnLootWindowOpened);
            EventHookManager.Subscribe("LOOT_BIND_CONFIRM", OnConfirmBindOnPickup);
            EventHookManager.Subscribe("CONFIRM_LOOT_ROLL", OnConfirmBindOnPickup);
            EventHookManager.Subscribe("START_LOOT_ROLL", OnLootRollStarted);
            EventHookManager.Subscribe("BAG_UPDATE", OnBagChanged);

            //// EventHookManager.Subscribe("DELETE_ITEM_CONFIRM", OnConfirmDeleteItem);
            //// EventHookManager.Subscribe("COMBAT_LOG_EVENT_UNFILTERED", OnCombatLog);
        }

        private void OnBagChanged(long timestamp, List<string> args)
        {
            AmeisenLogger.Instance.Log($"Event OnBagChanged: {JsonConvert.SerializeObject(args)}", LogLevel.Verbose);
            XMemory.Write(OffsetList.CvarMaxFps, 100);
            CharacterManager.Inventory.Update();
            CharacterManager.Equipment.Update();
            CharacterManager.UpdateCharacterGear();
            XMemory.Write(OffsetList.CvarMaxFps, Config.MaxFps);
        }

        private void OnLootRollStarted(long timestamp, List<string> args)
        {
            AmeisenLogger.Instance.Log($"Event OnLootRollStarted: {JsonConvert.SerializeObject(args)}", LogLevel.Verbose);

            if (int.TryParse(args[0], out int rollId))
            {
                string itemName = HookManager.GetLootRollItemLink(rollId);
                string json = HookManager.GetItemByName(itemName);
                WowBasicItem item = ItemFactory.ParseItem(json);
                item = ItemFactory.BuildSpecificItem(item);

                if (CharacterManager.IsItemAnImprovement(item, out IWowItem itemToReplace))
                {
                    AmeisenLogger.Instance.Log($"Would like to replace item {item?.Name} with {itemToReplace?.Name}, rolling need", LogLevel.Verbose);
                    HookManager.RollOnItem(rollId, RollType.Need);
                    return;
                }
            }

            HookManager.RollOnItem(rollId, RollType.Greed);
        }

        private void OnLootWindowOpened(long timestamp, List<string> args)
        {
            AmeisenLogger.Instance.Log($"Event OnLootWindowOpened: {JsonConvert.SerializeObject(args)}", LogLevel.Verbose);
            HookManager.LootEveryThing();
        }

        private void OnConfirmBindOnPickup(long timestamp, List<string> args)
        {
            AmeisenLogger.Instance.Log($"Event OnConfirmBindOnPickup: {JsonConvert.SerializeObject(args)}", LogLevel.Verbose);
            HookManager.CofirmBop();
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
                XMemory.Process.Close();
            }

            AmeisenLogger.Instance.Log($"Stopping AmeisenBot...", LogLevel.Master);
            AmeisenLogger.Instance.Stop();
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
                string filepath = Path.Combine(BotDataPath, AccountName, $"botpos.json");
                if (File.Exists(filepath))
                {
                    try
                    {
                        string rawRect = File.ReadAllText(filepath);
                        Rect rect = JsonConvert.DeserializeObject<Rect>(rawRect);

                        XMemory.SetWindowPosition(Process.GetCurrentProcess().MainWindowHandle, rect);
                        AmeisenLogger.Instance.Log($"Loaded bot window position: {rawRect}", LogLevel.Verbose);
                    }
                    catch (Exception e)
                    {
                        AmeisenLogger.Instance.Log($"Failed to set bot window position:\n{e.ToString()}", LogLevel.Error);
                    }
                }
            }
        }

        private void LoadWowWindowPosition()
        {
            if (AccountName.Length > 0)
            {
                string filepath = Path.Combine(BotDataPath, AccountName, $"wowpos.json");
                if (File.Exists(filepath))
                {
                    try
                    {
                        string rawRect = File.ReadAllText(filepath);
                        Rect rect = JsonConvert.DeserializeObject<Rect>(rawRect);

                        XMemory.SetWindowPosition(XMemory.Process.MainWindowHandle, rect);
                        AmeisenLogger.Instance.Log($"Loaded wow window position: {rawRect}", LogLevel.Verbose);
                    }
                    catch (Exception e)
                    {
                        AmeisenLogger.Instance.Log($"Failed to set wow window position:\n{e.ToString()}", LogLevel.Error);
                    }
                }
            }
        }

        private void SaveBotWindowPosition()
        {
            try
            {
                string filepath = Path.Combine(BotDataPath, AccountName, $"botpos.json");
                Rect rect = XMemory.GetWindowPosition(Process.GetCurrentProcess().MainWindowHandle);
                File.WriteAllText(filepath, JsonConvert.SerializeObject(rect));
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
                string filepath = Path.Combine(BotDataPath, AccountName, $"wowpos.json");
                Rect rect = XMemory.GetWindowPosition(XMemory.Process.MainWindowHandle);
                File.WriteAllText(filepath, JsonConvert.SerializeObject(rect));
            }
            catch (Exception e)
            {
                AmeisenLogger.Instance.Log($"Failed to save wow window position:\n{e.ToString()}", LogLevel.Error);
            }
        }

        private void StateMachineTimerTick(object sender, ElapsedEventArgs e)
        {
            // only start one timer tick at a time
            if (Interlocked.CompareExchange(ref stateMachineTimerBusy, 1, 0) == 1)
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

        private void OnCombatLog(long timestamp, List<string> args)
        {
            // analyze the combat log
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

        private void OnResurrectRequest(long timestamp, List<string> args)
        {
            AmeisenLogger.Instance.Log($"Event OnResurrectRequest: {JsonConvert.SerializeObject(args)}", LogLevel.Verbose);
            HookManager.AcceptResurrect();
        }

        private void OnSummonRequest(long timestamp, List<string> args)
        {
            AmeisenLogger.Instance.Log($"Event OnSummonRequest: {JsonConvert.SerializeObject(args)}", LogLevel.Verbose);
            HookManager.AcceptSummon();
        }
    }
}