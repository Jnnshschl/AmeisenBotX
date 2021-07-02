using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Fsm.Enums;
using AmeisenBotX.Core.Fsm.States;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Fsm
{
    public delegate void StateMachineOverride(BotState botState);

    public class AmeisenBotFsm
    {
        public AmeisenBotFsm(AmeisenBotConfig config, AmeisenBotInterfaces bot)
        {
            AmeisenLogger.I.Log("StateMachine", "Starting AmeisenBotStateMachine", LogLevel.Verbose);

            Config = config;
            Bot = bot;

            LastState = BotState.None;

            States = new()
            {
                { BotState.None, new StateNone(this, config, Bot) },
                { BotState.Attacking, new StateAttacking(this, config, Bot) },
                { BotState.Battleground, new StateBattleground(this, config, Bot) },
                { BotState.Dead, new StateDead(this, config, Bot) },
                { BotState.Dungeon, new StateDungeon(this, config, Bot) },
                { BotState.Eating, new StateEating(this, config, Bot) },
                { BotState.Following, new StateFollowing(this, config, Bot) },
                { BotState.Ghost, new StateGhost(this, config, Bot) },
                { BotState.Grinding, new StateGrinding(this, config, Bot) },
                { BotState.Idle, new StateIdle(this, config, Bot) },
                { BotState.Job, new StateJob(this, config, Bot) },
                { BotState.LoadingScreen, new StateLoadingScreen(this, config, Bot) },
                { BotState.Login, new StateLogin(this, config, Bot) },
                { BotState.Looting, new StateLooting(this, config, Bot) },
                { BotState.Questing, new StateQuesting(this, config, Bot) },
                { BotState.Repairing, new StateRepairing(this, config, Bot) },
                { BotState.Selling, new StateSelling(this, config, Bot) },
                { BotState.StartWow, new StateStartWow(this, config, Bot) }
            };

            AntiAfkEvent = new(TimeSpan.FromMilliseconds(Config.AntiAfkMs), Bot.Character.AntiAfk);
            RenderSwitchEvent = new(TimeSpan.FromSeconds(1));

            CurrentState = States.First();
            CurrentState.Value.Enter();
        }

        public event Action OnStateMachineStateChanged;

        public event Action OnStateMachineTick;

        public event StateMachineOverride OnStateOverride;

        public KeyValuePair<BotState, BasicState> CurrentState { get; protected set; }

        public WowMapId LastDiedMap { get; internal set; }

        public Vector3 LastDiedPosition { get; internal set; }

        public BotState LastState { get; protected set; }

        public bool ShouldExit { get; set; }

        public BotState StateOverride { get; set; }

        public Dictionary<BotState, BasicState> States { get; protected set; }

        public bool WowCrashed { get; internal set; }

        internal AmeisenBotInterfaces Bot { get; }

        private TimegatedEvent AntiAfkEvent { get; set; }

        private AmeisenBotConfig Config { get; }

        private TimegatedEvent RenderSwitchEvent { get; set; }

        public void Execute()
        {
            // Override states
            if (CurrentState.Key != BotState.None
                && CurrentState.Key != BotState.StartWow)
            {
                // Handle Wow crash
                if ((Bot.Memory.Process == null || Bot.Memory.Process.HasExited)
                    && SetState(BotState.None))
                {
                    AmeisenLogger.I.Log("StateMachine", "WoW crashed", LogLevel.Verbose);

                    WowCrashed = true;
                    GetState<StateIdle>().FirstStart = true;

                    Bot.Movement.Reset();
                    Bot.Events.Stop();
                    return;
                }

                Bot.Wow.Tick();

                AntiAfkEvent.Run();

                if (CurrentState.Key != BotState.Login
                    && Bot.Objects != null)
                {
                    if (!Bot.Objects.IsWorldLoaded)
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
                        if (Bot.Player != null)
                        {
                            Bot.Movement.Execute();

                            // handle event subbing
                            Bot.Events.Tick();

                            if (Bot.Player.IsDead)
                            {
                                // we are dead, state needs to release the spirit
                                if (SetState(BotState.Dead, true))
                                {
                                    OnStateOverride?.Invoke(CurrentState.Key);
                                    return;
                                }
                            }
                            else if (Bot.Player.IsGhost
                                && SetState(BotState.Ghost, true))
                            {
                                OnStateOverride?.Invoke(CurrentState.Key);
                                return;
                            }

                            // we cant fight when we are dead or a ghost
                            if (CurrentState.Key != BotState.Dead
                                && CurrentState.Key != BotState.Ghost
                                && !Bot.Globals.IgnoreCombat
                                && !(Config.IgnoreCombatWhileMounted && Bot.Player.IsMounted)
                                && (Bot.Globals.ForceCombat || Bot.Player.IsInCombat || IsAnyPartymemberInCombat()
                                || Bot.GetEnemiesInCombatWithParty<WowUnit>(Bot.Player.Position, 100.0f).Any()))
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
                        IntPtr foregroundWindow = Bot.Memory.GetForegroundWindow();
                        Bot.Wow.WowSetRenderState(foregroundWindow == Bot.Memory.Process.MainWindowHandle);
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
            return Bot.Objects.WowObjects.OfType<WowUnit>()
                       .Where(e => e.IsLootable
                           && !GetState<StateLooting>().UnitsAlreadyLootedList.Contains(e.Guid)
                           && e.Position.GetDistance(Bot.Player.Position) < Config.LootUnitsRadius);
        }

        internal bool IsAnyPartymemberInCombat()
        {
            return !Config.OnlySupportMaster && Bot.Objects.WowObjects.OfType<WowPlayer>()
                       .Where(e => Bot.Objects.PartymemberGuids.Contains(e.Guid) && e.Position.GetDistance(Bot.Player.Position) < Config.SupportRange)
                       .Any(r => r.IsInCombat);
        }
    }
}