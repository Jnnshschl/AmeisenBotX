using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Fsm.Enums;
using AmeisenBotX.Core.Fsm.States;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using AmeisenBotX.Memory;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Fsm
{
    public delegate void StateMachineOverride(BotState botState);

    public class AmeisenBotFsm
    {
        public AmeisenBotFsm(string botDataPath, AmeisenBotConfig config, WowInterface wowInterface)
        {
            AmeisenLogger.I.Log("StateMachine", "Starting AmeisenBotStateMachine", LogLevel.Verbose);

            BotDataPath = botDataPath;
            Config = config;
            WowInterface = wowInterface;

            LastState = BotState.None;

            States = new()
            {
                { BotState.None, new StateNone(this, config, WowInterface) },
                { BotState.Attacking, new StateAttacking(this, config, WowInterface) },
                { BotState.Battleground, new StateBattleground(this, config, WowInterface) },
                { BotState.Dead, new StateDead(this, config, WowInterface) },
                { BotState.Dungeon, new StateDungeon(this, config, WowInterface) },
                { BotState.Eating, new StateEating(this, config, WowInterface) },
                { BotState.Following, new StateFollowing(this, config, WowInterface) },
                { BotState.Ghost, new StateGhost(this, config, WowInterface) },
                { BotState.Grinding, new StateGrinding(this, config, WowInterface) },
                { BotState.Idle, new StateIdle(this, config, WowInterface) },
                { BotState.Job, new StateJob(this, config, WowInterface) },
                { BotState.LoadingScreen, new StateLoadingScreen(this, config, WowInterface) },
                { BotState.Login, new StateLogin(this, config, WowInterface) },
                { BotState.Looting, new StateLooting(this, config, WowInterface) },
                { BotState.Questing, new StateQuesting(this, config, WowInterface) },
                { BotState.Repairing, new StateRepairing(this, config, WowInterface) },
                { BotState.Selling, new StateSelling(this, config, WowInterface) },
                { BotState.StartWow, new StateStartWow(this, config, WowInterface) }
            };

            AntiAfkEvent = new(TimeSpan.FromMilliseconds(Config.AntiAfkMs), WowInterface.CharacterManager.AntiAfk);
            RenderSwitchEvent = new(TimeSpan.FromSeconds(1));

            CurrentState = States.First();
            CurrentState.Value.Enter();
        }

        public event Action OnStateMachineStateChanged;

        public event Action OnStateMachineTick;

        public event StateMachineOverride OnStateOverride;

        public string BotDataPath { get; }

        public KeyValuePair<BotState, BasicState> CurrentState { get; protected set; }

        public WowMapId LastDiedMap { get; internal set; }

        public Vector3 LastDiedPosition { get; internal set; }

        public BotState LastState { get; protected set; }

        public TimegatedEvent MovementEvent { get; set; }

        public string PlayerName { get; internal set; }

        public bool ShouldExit { get; set; }

        public BotState StateOverride { get; set; }

        public Dictionary<BotState, BasicState> States { get; protected set; }

        public bool WowCrashed { get; internal set; }

        internal WowInterface WowInterface { get; }

        private TimegatedEvent AntiAfkEvent { get; set; }

        private AmeisenBotConfig Config { get; }

        private TimegatedEvent RenderSwitchEvent { get; set; }

        public void Execute()
        {
            // Override states
            // --------------->
            if (CurrentState.Key != BotState.None
                && CurrentState.Key != BotState.StartWow)
            {
                // Handle Wow crash
                // ---------------- >
                if ((WowInterface.XMemory.Process == null || WowInterface.XMemory.Process.HasExited)
                    && SetState(BotState.None))
                {
                    AmeisenLogger.I.Log("StateMachine", "WoW crashed", LogLevel.Verbose);

                    WowCrashed = true;
                    GetState<StateIdle>().FirstStart = true;

                    WowInterface.MovementEngine.Reset();
                    WowInterface.EventHookManager.Stop();
                    return;
                }

                AntiAfkEvent.Run();

                if (CurrentState.Key != BotState.Login
                    && WowInterface.ObjectManager != null)
                {
                    if (!WowInterface.ObjectManager.RefreshIsWorldLoaded())
                    {
                        if (SetState(BotState.LoadingScreen, true))
                        {
                            OnStateOverride?.Invoke(CurrentState.Key);
                            AmeisenLogger.I.Log("StateMachine", "World is not loaded", LogLevel.Verbose);
                            return;
                        }
                    }
                    else
                    {
                        WowInterface.ObjectManager.UpdateWowObjects();

                        if (WowInterface.Player != null)
                        {
                            WowInterface.MovementEngine.Execute();

                            // handle event subbing
                            WowInterface.EventHookManager.ExecutePendingLua();

                            if (WowInterface.Player.IsDead)
                            {
                                // we are dead, state needs to release the spirit
                                if (SetState(BotState.Dead, true))
                                {
                                    OnStateOverride?.Invoke(CurrentState.Key);
                                    return;
                                }
                            }
                            else if (WowInterface.Player.IsGhost
                                && SetState(BotState.Ghost, true))
                            {
                                OnStateOverride?.Invoke(CurrentState.Key);
                                return;
                            }

                            // we cant fight when we are dead or a ghost
                            if (CurrentState.Key != BotState.Dead
                                && CurrentState.Key != BotState.Ghost
                                && !WowInterface.Globals.IgnoreCombat
                                && !(Config.IgnoreCombatWhileMounted && WowInterface.Player.IsMounted)
                                && (WowInterface.Globals.ForceCombat || WowInterface.Player.IsInCombat || IsAnyPartymemberInCombat()
                                || WowInterface.ObjectManager.GetEnemiesInCombatWithUs<WowUnit>(WowInterface.Player.Position, 100.0f).Any()))
                            {
                                if (SetState(BotState.Attacking, true))
                                {
                                    OnStateOverride?.Invoke(CurrentState.Key);
                                    return;
                                }
                            }
                        }
                    }

                    // auto disable rendering when not in focus
                    if (Config.AutoDisableRender && RenderSwitchEvent.Run())
                    {
                        IntPtr foregroundWindow = XMemory.GetForegroundWindow();
                        WowInterface.HookManager.WowSetRenderState(foregroundWindow == WowInterface.XMemory.Process.MainWindowHandle);
                    }
                }
            }

            // execute the State and Movement
            CurrentState.Value.Execute();
            OnStateMachineTick?.Invoke();
        }

        public T GetState<T>() where T : BasicState
        {
            return (T)States.FirstOrDefault(e => e.Value.GetType() == typeof(T)).Value;
        }

        public bool SetState(BotState state, bool ignoreExit = false)
        {
            if (CurrentState.Key == state)
            {
                // we are already in this state
                return false;
            }

            AmeisenLogger.I.Log("StateMachine", $"Changing State to {state}");

            LastState = CurrentState.Key;

            // this is used by the combat state because
            // it will override any existing state
            if (!ignoreExit)
            {
                CurrentState.Value.Leave();
            }

            CurrentState = States.First(s => s.Key == state);
            CurrentState.Value.Enter();

            OnStateMachineStateChanged?.Invoke();
            return true;
        }

        internal IEnumerable<WowUnit> GetNearLootableUnits()
        {
            return WowInterface.ObjectManager.WowObjects.OfType<WowUnit>()
                       .Where(e => e.IsLootable
                           && !GetState<StateLooting>().UnitsAlreadyLootedList.Contains(e.Guid)
                           && e.Position.GetDistance(WowInterface.Player.Position) < Config.LootUnitsRadius);
        }

        internal bool IsAnyPartymemberInCombat()
        {
            return !Config.OnlySupportMaster && WowInterface.ObjectManager.WowObjects.OfType<WowPlayer>()
                       .Where(e => WowInterface.ObjectManager.PartymemberGuids.Contains(e.Guid) && e.Position.GetDistance(WowInterface.Player.Position) < Config.SupportRange)
                       .Any(r => r.IsInCombat);
        }
    }
}