using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.StateMachine.States;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.StateMachine
{
    public class AmeisenBotStateMachine
    {
        public AmeisenBotStateMachine(string botDataPath, AmeisenBotConfig config, WowInterface wowInterface)
        {
            AmeisenLogger.Instance.Log("StateMachine", "Starting AmeisenBotStateMachine...", LogLevel.Verbose);

            BotDataPath = botDataPath;
            Config = config;
            WowInterface = wowInterface;

            LastObjectUpdate = DateTime.Now;
            LastGhostCheck = DateTime.Now;
            LastEventPull = DateTime.Now;

            LastState = BotState.None;
            UnitLootList = new Queue<ulong>();

            States = new Dictionary<BotState, BasicState>()
            {
                { BotState.None, new StateNone(this, config) },
                { BotState.StartWow, new StateStartWow(this, config, WowInterface) },
                { BotState.Login, new StateLogin(this, config, WowInterface) },
                { BotState.LoadingScreen, new StateLoadingScreen(this, WowInterface) },
                { BotState.Idle, new StateIdle(this, config, WowInterface, UnitLootList) },
                { BotState.Dead, new StateDead(this, config, WowInterface) },
                { BotState.Ghost, new StateGhost(this, config, WowInterface) },
                { BotState.Following, new StateFollowing(this, config, WowInterface) },
                { BotState.Attacking, new StateAttacking(this, config, WowInterface) },
                { BotState.Repairing, new StateRepairing(this, config, WowInterface) },
                { BotState.Selling, new StateSelling(this, config, WowInterface) },
                { BotState.Healing, new StateEating(this, config, WowInterface) },
                { BotState.InsideAoeDamage, new StateInsideAoeDamage(this, config, WowInterface) },
                { BotState.Looting, new StateLooting(this, config, WowInterface, UnitLootList) },
                { BotState.Battleground, new StateBattleground(this, config, WowInterface) },
                { BotState.Job, new StateJob(this, WowInterface) }
            };

            CurrentState = States.First();
            CurrentState.Value.Enter();
        }

        public delegate void StateMachineStateChange();

        public delegate void StateMachineTick();

        public event StateMachineStateChange OnStateMachineStateChanged;

        public event StateMachineTick OnStateMachineTick;

        public string BotDataPath { get; }

        public KeyValuePair<BotState, BasicState> CurrentState { get; private set; }

        public BotState LastState { get; private set; }

        public string PlayerName { get; internal set; }

        internal Queue<ulong> UnitLootList { get; set; }

        internal WowInterface WowInterface { get; }

        private AmeisenBotConfig Config { get; }

        private DateTime LastEventPull { get; set; }

        private DateTime LastGhostCheck { get; set; }

        private DateTime LastObjectUpdate { get; set; }

        private Dictionary<BotState, BasicState> States { get; }

        public void Execute()
        {
            if (WowInterface.XMemory.Process == null || WowInterface.XMemory.Process.HasExited)
            {
                AmeisenLogger.Instance.Log("StateMachine", "WoW crashed...", LogLevel.Verbose);
                SetState(BotState.None);
            }

            if (WowInterface.ObjectManager != null)
            {
                if (!WowInterface.ObjectManager.IsWorldLoaded)
                {
                    SetState(BotState.LoadingScreen);
                    WowInterface.MovementEngine.Reset();
                    return;
                }
                else
                {
                    HandleObjectUpdates();
                    HandleEventPull();

                    if (WowInterface.ObjectManager.Player != null)
                    {
                        HandlePlayerDeadOrGhostState();

                        if (CurrentState.Key != BotState.Dead && CurrentState.Key != BotState.Ghost)
                        {
                            if (Config.AutoDodgeAoeSpells
                                && BotUtils.IsPositionInsideAoeSpell(WowInterface.ObjectManager.Player.Position, WowInterface.ObjectManager.WowObjects.OfType<WowDynobject>().ToList()))
                            {
                                SetState(BotState.InsideAoeDamage);
                            }

                            if (WowInterface.ObjectManager.Player.IsInCombat || IsAnyPartymemberInCombat())
                            {
                                SetState(BotState.Attacking, true);
                            }
                        }
                    }
                }
            }

            WowInterface.CharacterManager.AntiAfk();

            // used for ui updates
            OnStateMachineTick?.Invoke();
            CurrentState.Value.Execute();
        }

        internal bool IsAnyPartymemberInCombat(double distance = 50)
            => WowInterface.ObjectManager.WowObjects.OfType<WowPlayer>()
            .Where(e => WowInterface.ObjectManager.PartymemberGuids.Contains(e.Guid))
            .Any(r => r.Position.GetDistance(WowInterface.ObjectManager.Player.Position) < distance && r.IsInCombat);

        internal bool IsOnBattleground()
            => WowInterface.ObjectManager.MapId == 30
            || WowInterface.ObjectManager.MapId == 489
            || WowInterface.ObjectManager.MapId == 529
            || WowInterface.ObjectManager.MapId == 566
            || WowInterface.ObjectManager.MapId == 607;

        internal void SetState(BotState state, bool ignoreExit = false)
        {
            if (CurrentState.Key == state)
            {
                // we are already in this state
                return;
            }

            LastState = CurrentState.Key;

            if (!ignoreExit)
            {
                CurrentState.Value.Exit();
            }

            CurrentState = States.First(s => s.Key == state);
            CurrentState.Value.Enter();

            OnStateMachineStateChanged?.Invoke();
        }

        private void HandleEventPull()
        {
            if (WowInterface.EventHookManager.IsSetUp
                && LastEventPull + TimeSpan.FromSeconds(1) < DateTime.Now)
            {
                WowInterface.EventHookManager.ReadEvents();
                LastEventPull = DateTime.Now;
            }
        }

        private void HandleObjectUpdates()
        {
            if (LastObjectUpdate - TimeSpan.FromMilliseconds(Config.ObjectUpdateMs) < DateTime.Now
                && CurrentState.Key != BotState.None
                && CurrentState.Key != BotState.StartWow
                && CurrentState.Key != BotState.Login
                && CurrentState.Key != BotState.LoadingScreen)
            {
                WowInterface.ObjectManager.UpdateWowObjects();
                LastObjectUpdate = DateTime.Now;
            }
        }

        private void HandlePlayerDeadOrGhostState()
        {
            if (WowInterface.ObjectManager.Player.IsDead)
            {
                SetState(BotState.Dead);
            }
            else
            {
                if (LastGhostCheck + TimeSpan.FromSeconds(8) < DateTime.Now)
                {
                    bool isGhost = WowInterface.HookManager.IsGhost("player");
                    LastGhostCheck = DateTime.Now;

                    if (isGhost)
                    {
                        SetState(BotState.Ghost);
                    }
                }
            }
        }
    }
}