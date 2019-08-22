using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Event;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.OffsetLists;
using AmeisenBotX.Core.StateMachine;
using AmeisenBotX.Core.StateMachine.CombatClasses;
using AmeisenBotX.Memory;
using AmeisenBotX.Memory.Win32;
using AmeisenBotX.Pathfinding;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Timers;
using Timer = System.Timers.Timer;

namespace AmeisenBotX.Core
{
    public class AmeisenBot
    {
        public AmeisenBotConfig Config { get; }

        public AmeisenBotStateMachine StateMachine { get; set; }
        public ObjectManager ObjectManager { get; set; }
        public CharacterManager CharacterManager { get; set; }
        public HookManager HookManager { get; set; }
        public EventHookManager EventHookManager { get; set; }
        public CacheManager CacheManager { get; set; }
        public IPathfindingHandler PathfindingHandler { get; set; }
        public ICombatClass CombatClass { get; set; }
        public Process WowProcess { get; }

        public IOffsetList OffsetList { get; }

        private double currentExecutionMs;

        public double CurrentExecutionMs
        {
            get
            {
                double avgTickTime = Math.Round(currentExecutionMs / CurrentExecutionCount, 2);
                CurrentExecutionCount = 0;
                return avgTickTime;
            }
            private set { currentExecutionMs = value; }
        }

        private int CurrentExecutionCount { get; set; }

        private XMemory XMemory { get; }

        private Timer StateMachineTimer { get; }

        private int stateMachineTimerBusy;

        public AmeisenBot(AmeisenBotConfig config)
        {
            CurrentExecutionMs = 0;
            CurrentExecutionCount = 0;

            stateMachineTimerBusy = 0;

            StateMachineTimer = new Timer(config.StateMachineTickMs);
            StateMachineTimer.Elapsed += StateMachineTimerTick;

            Config = config;
            XMemory = new XMemory();
            OffsetList = new OffsetList335a();

            CacheManager = new CacheManager(config);
            ObjectManager = new ObjectManager(XMemory, OffsetList, CacheManager);
            CharacterManager = new CharacterManager(XMemory, OffsetList, ObjectManager);
            HookManager = new HookManager(XMemory, OffsetList, ObjectManager, CacheManager);
            EventHookManager = new EventHookManager(HookManager);
            PathfindingHandler = new NavmeshServerClient(Config.NavmeshServerIp, Config.NameshServerPort);

            switch(Config.CombatClassName.ToUpper())
            {
                case "WARRIORARMS":
                    CombatClass = new WarriorArms(ObjectManager, CharacterManager, HookManager);
                    break;

                default:
                    CombatClass = null;
                    break;
            }

            StateMachine = new AmeisenBotStateMachine(WowProcess, Config, XMemory, OffsetList, ObjectManager, CharacterManager, HookManager, EventHookManager, CacheManager, PathfindingHandler, CombatClass);

            if (!Directory.Exists(Config.BotDataPath)) Directory.CreateDirectory(Config.BotDataPath);
        }

        public void Start()
        {
            StateMachineTimer.Start();
            CacheManager.LoadFromFile();

            EventHookManager.Subscribe("PARTY_INVITE_REQUEST", OnPartyInvitation);
            EventHookManager.Subscribe("RESURRECT_REQUEST", OnResurrectRequest);
            EventHookManager.Subscribe("CONFIRM_SUMMON", OnSummonRequest);
            EventHookManager.Subscribe("READY_CHECK", OnReadyCheck);
            //EventHookManager.Subscribe("COMBAT_LOG_EVENT_UNFILTERED", OnCombatLog);
        }

        public void Stop()
        {
            StateMachineTimer.Stop();
            CacheManager.SaveToFile();

            HookManager.DisposeHook();
            EventHookManager.Stop();

            if (Config.SaveWowWindowPosition) SaveWowWindowPosition();
            if (Config.SaveBotWindowPosition) SaveBotWindowPosition();
        }

        private void StateMachineTimerTick(object sender, ElapsedEventArgs e)
        {
            if (Interlocked.CompareExchange(ref stateMachineTimerBusy, 1, 0) == 1) return;

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

        private void SaveWowWindowPosition()
        {
            string filepath = Path.Combine(Config.BotDataPath, $"wowpos_{ObjectManager.Player.Name}.json");
            try
            {
                Rect rect = XMemory.GetWindowPosition(XMemory.Process.MainWindowHandle);
                File.WriteAllText(filepath, JsonConvert.SerializeObject(rect));
            }
            catch { }
        }

        private void SaveBotWindowPosition()
        {
            string filepath = Path.Combine(Config.BotDataPath, $"botpos_{ObjectManager.Player.Name}.json");
            try
            {
                Rect rect = XMemory.GetWindowPosition(Process.GetCurrentProcess().MainWindowHandle);
                File.WriteAllText(filepath, JsonConvert.SerializeObject(rect));
            }
            catch { }
        }

        #region WowEvents
        private void OnPartyInvitation(long timestamp, List<string> args)
            => HookManager.AcceptPartyInvite();

        private void OnSummonRequest(long timestamp, List<string> args)
            => HookManager.AcceptSummon();

        private void OnResurrectRequest(long timestamp, List<string> args)
            => HookManager.AcceptResurrect();

        private void OnReadyCheck(long timestamp, List<string> args)
            => HookManager.CofirmReadyCheck(true);

        private void OnCombatLog(long timestamp, List<string> args)
        {
            // analyze the combat log
        }
        #endregion
    }
}