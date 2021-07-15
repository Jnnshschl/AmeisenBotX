using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Fsm.Enums;
using AmeisenBotX.Core.Fsm.States;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Fsm
{
    public class AmeisenBotFsm
    {
        public AmeisenBotFsm(AmeisenBotConfig config, AmeisenBotInterfaces bot)
        {
            AmeisenLogger.I.Log("StateMachine", "Starting AmeisenBotStateMachine", LogLevel.Verbose);

            Config = config;
            Bot = bot;

            States = new()
            {
                { BotState.None, new StateNone(this, Config, Bot) },
                { BotState.Battleground, new StateBattleground(this, Config, Bot) },
                { BotState.Combat, new StateCombat(this, Config, Bot) },
                { BotState.Dead, new StateDead(this, Config, Bot) },
                { BotState.Dungeon, new StateDungeon(this, Config, Bot) },
                { BotState.Eating, new StateEating(this, Config, Bot) },
                { BotState.Following, new StateFollowing(this, Config, Bot) },
                { BotState.Ghost, new StateGhost(this, Config, Bot) },
                { BotState.Grinding, new StateGrinding(this, Config, Bot) },
                { BotState.Idle, new StateIdle(this, Config, Bot) },
                { BotState.Job, new StateJob(this, Config, Bot) },
                { BotState.LoadingScreen, new StateLoadingScreen(this, Config, Bot) },
                { BotState.Login, new StateLogin(this, Config, Bot) },
                { BotState.Looting, new StateLooting(this, Config, Bot) },
                { BotState.Questing, new StateQuesting(this, Config, Bot) },
                { BotState.Repairing, new StateRepairing(this, Config, Bot) },
                { BotState.Selling, new StateSelling(this, Config, Bot) },
                { BotState.StartWow, new StateStartWow(this, Config, Bot) },
                { BotState.StateTalkToQuestgivers, new StateTalkToQuestgivers(this, Config, Bot) }
                // add new state here
            };

            AntiAfkEvent = new(TimeSpan.FromMilliseconds(Config.AntiAfkMs), Bot.Character.AntiAfk);
            RenderSwitchEvent = new(TimeSpan.FromSeconds(1));

            CurrentState = States.First();
            CurrentState.Value.Enter();
        }

        /// <summary>
        /// Will be fired when our current state changes.
        /// </summary>
        public event Action OnStateMachineStateChanged;

        /// <summary>
        /// Will be fired when the statemachine completed a tick.
        /// </summary>
        public event Action OnStateMachineTick;

        /// <summary>
        /// Will be fired when the current state was overridden. For example when we died.
        /// </summary>
        public event Action<BotState> OnStateOverride;

        /// <summary>
        /// Pair of the current state id and its corresponding class.
        /// </summary>
        public KeyValuePair<BotState, BasicState> CurrentState { get; private set; }

        /// <summary>
        /// Current overrider state.
        /// </summary>
        public BotState StateOverride { get; set; }

        /// <summary>
        /// Returns all available states.
        /// </summary>
        public Dictionary<BotState, BasicState> States { get; }

        /// <summary>
        /// Returns true if the bot thinks wow has crashed.
        /// </summary>
        public bool WowCrashed { get; internal set; }

        internal AmeisenBotInterfaces Bot { get; }

        private TimegatedEvent AntiAfkEvent { get; set; }

        private AmeisenBotConfig Config { get; }

        private TimegatedEvent RenderSwitchEvent { get; set; }

        /// <summary>
        /// Runs a tick of the bots fsm.
        /// </summary>
        public void Execute()
        {
            // no need to tick the states None and StartWow as the bot is not running
            if (CurrentState.Key is not BotState.None and not BotState.StartWow)
            {
                // wow crashed or was closed by the user
                if ((Bot.Memory.Process == null || Bot.Memory.Process.HasExited)
                    && SetState(BotState.None))
                {
                    AmeisenLogger.I.Log("StateMachine", "WoW crashed", LogLevel.Verbose);

                    WowCrashed = true;
                    GetState<StateIdle>().FirstStart = true;

                    Bot.Movement.Reset();
                    Bot.Wow.Events.Stop();
                    return;
                }

                // update the wow interface
                Bot.Wow.Tick();

                // make sure we dont go afk
                AntiAfkEvent.Run();

                if (CurrentState.Key != BotState.Login && Bot.Player != null)
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
                        Bot.Movement.Execute();

                        // handle event subbing
                        Bot.Wow.Events.Tick();

                        // auto disable rendering when not in focus
                        if (Config.AutoDisableRender && RenderSwitchEvent.Run())
                        {
                            IntPtr foregroundWindow = Bot.Memory.GetForegroundWindow();
                            Bot.Wow.WowSetRenderState(foregroundWindow == Bot.Memory.Process.MainWindowHandle);
                        }

                        // override states, for example when we die, we need to revive
                        // there is no way we can be a ghost or stay in combat
                        if (Bot.Player.IsDead)
                        {
                            if (SetState(BotState.Dead, true))
                            {
                                OnStateOverride?.Invoke(CurrentState.Key);
                            }
                        }
                        else if (Bot.Player.IsGhost)
                        {
                            if (SetState(BotState.Ghost, true))
                            {
                                OnStateOverride?.Invoke(CurrentState.Key);
                            }
                        }
                        else if (!Bot.Globals.IgnoreCombat
                                && !(Config.IgnoreCombatWhileMounted && Bot.Player.IsMounted)
                                && (Bot.Globals.ForceCombat || Bot.Player.IsInCombat || GetState<StateCombat>().IsAnyPartymemberInCombat()
                                || Bot.GetEnemiesInCombatWithParty<IWowUnit>(Bot.Player.Position, 100.0f).Any()))
                        {
                            if (SetState(BotState.Combat, true))
                            {
                                OnStateOverride?.Invoke(CurrentState.Key);
                            }
                        }
                    }
                }
            }

            // execute the current state
            CurrentState.Value.Execute();
            OnStateMachineTick?.Invoke();
        }

        /// <summary>
        /// Returns a state instance by its type.
        /// </summary>
        /// <typeparam name="T">Type of the state</typeparam>
        /// <returns>State instance or null if not found</returns>
        public T GetState<T>() where T : BasicState
        {
            return (T)States.FirstOrDefault(e => e.Value.GetType() == typeof(T)).Value;
        }

        /// <summary>
        /// Changes the current state of the bots fsm.
        /// </summary>
        /// <param name="state">State to change to</param>
        /// <param name="ignoreExit">If true, the states Leave() function won't be called. This is used to override states and resume them.</param>
        /// <returns>True when the state was changed, false if we are already in that state</returns>
        public bool SetState(BotState state, bool ignoreExit = false)
        {
            if (CurrentState.Key == state)
            {
                // we are already in this state
                return false;
            }

            AmeisenLogger.I.Log("StateMachine", $"Changing State to {state}");

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
    }
}