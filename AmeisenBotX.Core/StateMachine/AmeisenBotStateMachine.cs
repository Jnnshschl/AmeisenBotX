using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Statemachine.States;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Statemachine
{
    public class AmeisenBotStateMachine
    {
        public AmeisenBotStateMachine(string botDataPath, AmeisenBotConfig config, WowInterface wowInterface)
        {
            AmeisenLogger.Instance.Log("StateMachine", "Starting AmeisenBotStateMachine...", LogLevel.Verbose);

            BotDataPath = botDataPath;
            Config = config;
            WowInterface = wowInterface;

            LastGhostCheck = DateTime.Now;
            LastEventPull = DateTime.Now;

            LastState = BotState.None;
            UnitLootList = new Queue<ulong>();

            States = new Dictionary<BotState, BasicState>()
            {
                { BotState.None, new StateNone(this, config, WowInterface) },
                { BotState.Attacking, new StateAttacking(this, config, WowInterface) },
                { BotState.Battleground, new StateBattleground(this, config, WowInterface) },
                { BotState.Dead, new StateDead(this, config, WowInterface) },
                { BotState.Dungeon, new StateDungeon(this, config, WowInterface) },
                { BotState.Eating, new StateEating(this, config, WowInterface) },
                { BotState.Following, new StateFollowing(this, config, WowInterface) },
                { BotState.Ghost, new StateGhost(this, config, WowInterface) },
                { BotState.Idle, new StateIdle(this, config, WowInterface, UnitLootList) },
                { BotState.InsideAoeDamage, new StateInsideAoeDamage(this, config, WowInterface) },
                { BotState.Job, new StateJob(this, config, WowInterface) },
                { BotState.LoadingScreen, new StateLoadingScreen(this, config, WowInterface) },
                { BotState.Login, new StateLogin(this, config, WowInterface) },
                { BotState.Looting, new StateLooting(this, config, WowInterface, UnitLootList) },
                { BotState.Repairing, new StateRepairing(this, config, WowInterface) },
                { BotState.Selling, new StateSelling(this, config, WowInterface) },
                { BotState.StartWow, new StateStartWow(this, config, WowInterface) }
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

        private Dictionary<BotState, BasicState> States { get; }

        public void Execute()
        {
            if (WowInterface.XMemory.Process == null || WowInterface.XMemory.Process.HasExited)
            {
                AmeisenLogger.Instance.Log("StateMachine", "WoW crashed...", LogLevel.Verbose);
                SetState(BotState.None);
            }

            if (WowInterface.ObjectManager != null && CurrentState.Key != BotState.LoadingScreen)
            {
                if (!WowInterface.ObjectManager.IsWorldLoaded)
                {
                    AmeisenLogger.Instance.Log("StateMachine", "World is not loaded...", LogLevel.Verbose);
                    SetState(BotState.LoadingScreen, true);
                    WowInterface.MovementEngine.Reset();
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
                                SetState(BotState.InsideAoeDamage, true);
                            }

                            if (WowInterface.ObjectManager.Player.IsInCombat || IsAnyPartymemberInCombat())
                            {
                                SetState(BotState.Attacking, true);
                            }
                        }
                    }
                }
            }

            // execute the State
            CurrentState.Value.Execute();

            // used for ui updates
            OnStateMachineTick?.Invoke();
        }

        internal bool IsAnyPartymemberInCombat()
            => WowInterface.ObjectManager.WowObjects.OfType<WowPlayer>()
            .Where(e => WowInterface.ObjectManager.PartymemberGuids.Contains(e.Guid))
            .Any(r => r.IsInCombat);

        internal bool IsInCapitalCity()
        {
            return false;
        }

        internal bool IsInDungeon()
            => WowInterface.ObjectManager.MapId == MapId.Deadmines;

        internal bool IsOnBattleground()
            => WowInterface.ObjectManager.MapId == MapId.AlteracValley
            || WowInterface.ObjectManager.MapId == MapId.WarsongGulch
            || WowInterface.ObjectManager.MapId == MapId.ArathiBasin
            || WowInterface.ObjectManager.MapId == MapId.EyeOfTheStorm
            || WowInterface.ObjectManager.MapId == MapId.StrandOfTheAncients;

        internal void SetState(BotState state, bool ignoreExit = false)
        {
            if (CurrentState.Key == state)
            {
                // we are already in this state
                return;
            }

            LastState = CurrentState.Key;

            // this is used by the combat state because
            // it will override any existing state
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
                && LastEventPull + TimeSpan.FromSeconds(Config.EventPullMs) < DateTime.Now)
            {
                WowInterface.EventHookManager.ReadEvents();
                LastEventPull = DateTime.Now;

                // anti AFK
                WowInterface.CharacterManager.AntiAfk();
            }
        }

        private void HandleObjectUpdates()
        {
            if (CurrentState.Key != BotState.None
                && CurrentState.Key != BotState.StartWow
                && CurrentState.Key != BotState.Login
                && CurrentState.Key != BotState.LoadingScreen)
            {
                WowInterface.ObjectManager.UpdateWowObjects();
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