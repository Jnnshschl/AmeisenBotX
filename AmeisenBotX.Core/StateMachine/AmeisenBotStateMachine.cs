using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Core.Statemachine.States;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Statemachine
{
    public delegate void StateMachineOverride(BotState botState);

    public delegate void StateMachineStateChange();

    public delegate void StateMachineTick();

    public class AmeisenBotStateMachine
    {
        public AmeisenBotStateMachine(string botDataPath, AmeisenBotConfig config, WowInterface wowInterface)
        {
            AmeisenLogger.Instance.Log("StateMachine", "Starting AmeisenBotStateMachine", LogLevel.Verbose);

            BotDataPath = botDataPath;
            Config = config;
            WowInterface = wowInterface;

            LastState = BotState.None;

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
                { BotState.Idle, new StateIdle(this, config, WowInterface) },
                { BotState.InsideAoeDamage, new StateInsideAoeDamage(this, config, WowInterface) },
                { BotState.Job, new StateJob(this, config, WowInterface) },
                { BotState.LoadingScreen, new StateLoadingScreen(this, config, WowInterface) },
                { BotState.Login, new StateLogin(this, config, WowInterface) },
                { BotState.Looting, new StateLooting(this, config, WowInterface) },
                { BotState.Questing, new StateQuesting(this, config, WowInterface) },
                { BotState.Repairing, new StateRepairing(this, config, WowInterface) },
                { BotState.Selling, new StateSelling(this, config, WowInterface) },
                { BotState.StartWow, new StateStartWow(this, config, WowInterface) }
            };

            GetState<StateStartWow>().OnWoWStarted += () => OnWowStarted?.Invoke();

            AntiAfkEvent = new TimegatedEvent(TimeSpan.FromMilliseconds(Config.AntiAfkMs), WowInterface.CharacterManager.AntiAfk);
            EventPullEvent = new TimegatedEvent(TimeSpan.FromMilliseconds(Config.EventPullMs), WowInterface.EventHookManager.Pull);
            GhostCheckEvent = new TimegatedEvent<bool>(TimeSpan.FromMilliseconds(Config.GhostCheckMs), () => WowInterface.ObjectManager.Player.Health == 1 && WowInterface.HookManager.IsGhost(WowLuaUnit.Player));
            RenderSwitchEvent = new TimegatedEvent(TimeSpan.FromSeconds(1));

            CurrentState = States.First();
            CurrentState.Value.Enter();
        }

        public event StateMachineStateChange OnStateMachineStateChanged;

        public event StateMachineTick OnStateMachineTick;

        public event StateMachineOverride OnStateOverride;

        public event Action OnWowStarted;

        public string BotDataPath { get; }

        public KeyValuePair<BotState, BasicState> CurrentState { get; protected set; }

        public MapId LastDiedMap { get; internal set; }

        public Vector3 LastDiedPosition { get; internal set; }

        public BotState LastState { get; protected set; }

        public string PlayerName { get; internal set; }

        public bool ShouldExit { get; set; }

        public BotState StateOverride { get; set; }

        public Dictionary<BotState, BasicState> States { get; protected set; }

        public bool WowCrashed { get; internal set; }

        internal WowInterface WowInterface { get; }

        private TimegatedEvent AntiAfkEvent { get; set; }

        private AmeisenBotConfig Config { get; }

        private TimegatedEvent EventPullEvent { get; set; }

        private TimegatedEvent<bool> GhostCheckEvent { get; set; }

        private TimegatedEvent RenderSwitchEvent { get; set; }

        public void Execute()
        {
            // Handle Wow crash
            // ---------------- >
            if ((WowInterface.XMemory.Process == null || WowInterface.XMemory.Process.HasExited)
                && SetState((int)BotState.None))
            {
                AmeisenLogger.Instance.Log("StateMachine", "WoW crashed", LogLevel.Verbose);

                WowCrashed = true;
                GetState<StateIdle>().FirstStart = true;

                WowInterface.MovementEngine.Reset();
                WowInterface.ObjectManager.WowObjects.Clear();
                WowInterface.EventHookManager.Stop();

                return;
            }

            // Override states
            // --------------->
            if (CurrentState.Key != BotState.None
                && CurrentState.Key != BotState.StartWow
                && CurrentState.Key != BotState.Login
                && WowInterface.ObjectManager != null)
            {
                WowInterface.ObjectManager.RefreshIsWorldLoaded();

                if (!WowInterface.ObjectManager.IsWorldLoaded)
                {
                    if (SetState(BotState.LoadingScreen, true))
                    {
                        OnStateOverride?.Invoke(CurrentState.Key);
                        AmeisenLogger.Instance.Log("StateMachine", "World is not loaded", LogLevel.Verbose);
                        return;
                    }
                }
                else
                {
                    WowInterface.ObjectManager.UpdateWowObjects();
                    EventPullEvent.Run();

                    if (WowInterface.ObjectManager.Player != null)
                    {
                        if (!WowInterface.ObjectManager.Player.IsCasting)
                        {
                            WowInterface.MovementEngine.Execute();
                        }

                        if (WowInterface.ObjectManager.Player.IsDead)
                        {
                            // we are dead, state needs to release the spirit
                            if (SetState(BotState.Dead, true))
                            {
                                OnStateOverride?.Invoke(CurrentState.Key);
                                return;
                            }
                        }
                        else if (GhostCheckEvent.Run(out bool isGhost)
                            && isGhost)
                        {
                            // we cant be a ghost if we are still dead
                            if (SetState(BotState.Ghost, true))
                            {
                                OnStateOverride?.Invoke(CurrentState.Key);
                                return;
                            }
                        }

                        // we cant fight nor do we receive damage when we are dead or a ghost
                        // so ignore these overrides
                        if (CurrentState.Key != BotState.Dead
                            && CurrentState.Key != BotState.Ghost)
                        {
                            if (Config.AutoDodgeAoeSpells
                                && BotUtils.IsPositionInsideAoeSpell(WowInterface.ObjectManager.Player.Position, WowInterface.ObjectManager.GetNearAoeSpells())
                                && SetState(BotState.InsideAoeDamage, true))
                            {
                                OnStateOverride(CurrentState.Key);
                                return;
                            }

                            // TODO: handle combat bug, sometimes when combat ends, the player stays in combat for no reason
                            if (!WowInterface.Globals.IgnoreCombat
                                && (WowInterface.ObjectManager.Player.IsInCombat
                                    || WowInterface.Globals.ForceCombat
                                    || IsAnyPartymemberInCombat()))
                            {
                                if (SetState(BotState.Attacking, true))
                                {
                                    OnStateOverride?.Invoke(CurrentState.Key);
                                    return;
                                }
                            }
                        }
                    }
                }

                if (CurrentState.Key == BotState.Idle
                    && CurrentState.Key != StateOverride
                    && StateOverride != BotState.None)
                {
                    SetState(StateOverride);
                }
            }

            AntiAfkEvent.Run();

            // auto disable rendering when not in focus
            if (Config.AutoDisableRender && RenderSwitchEvent.Run())
            {
                IntPtr foregroundWindow = WowInterface.XMemory.GetForegroundWindow();
                WowInterface.HookManager.SetRenderState(foregroundWindow == WowInterface.XMemory.Process.MainWindowHandle);
            }

            // execute the State and Movement
            CurrentState.Value.Execute();
            OnStateMachineTick?.Invoke();
        }

        public T GetState<T>() where T : BasicState
        {
            return (T)States.FirstOrDefault(e => e.GetType() == typeof(T)).Value;
        }

        public bool SetState(BotState state, bool ignoreExit = false)
        {
            if (CurrentState.Key == state)
            {
                // we are already in this state
                return false;
            }

            LastState = CurrentState.Key;

            // this is used by the combat state because
            // it will override any existing state
            if (!ignoreExit)
            {
                CurrentState.Value.Exit();
            }

            CurrentState = States.First(s => s.Key == state);

            if (!ignoreExit)
            {
                CurrentState.Value.Enter();
            }

            OnStateMachineStateChanged?.Invoke();
            return true;
        }

        internal IEnumerable<WowUnit> GetNearLootableUnits()
        {
            return WowInterface.ObjectManager.WowObjects.OfType<WowUnit>()
                       .Where(e => e.IsLootable
                                && !GetState<StateLooting>().UnitsAlreadyLootedList.Contains(e.Guid)
                                && e.Position.GetDistance(WowInterface.ObjectManager.Player.Position) < Config.LootUnitsRadius);
        }

        internal bool IsAnyPartymemberInCombat()
        {
            return WowInterface.ObjectManager.WowObjects.OfType<WowPlayer>()
                       .Where(e => WowInterface.ObjectManager.PartymemberGuids.Contains(e.Guid) && e.Position.GetDistance(WowInterface.ObjectManager.Player.Position) < Config.PartyCombatRange)
                       .Any(r => r.IsInCombat);
        }
    }
}