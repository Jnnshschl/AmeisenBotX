using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.OffsetLists;
using AmeisenBotX.Core.StateMachine;
using AmeisenBotX.Memory;
using System;
using System.Diagnostics;
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
        public CacheManager CacheManager { get; set; }

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
            StateMachine = new AmeisenBotStateMachine(WowProcess, Config, XMemory, OffsetList, ObjectManager, CharacterManager, HookManager, CacheManager);
        }

        public void Start()
        {
            StateMachineTimer.Start();
            CacheManager.LoadFromFile();
        }

        public void Stop()
        {
            StateMachineTimer.Stop();
            CacheManager.SaveToFile();
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
    }
}