using AmeisenBotX.Core.Statemachine.States;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Statemachine
{
    public abstract class AbstractStateMachine
    {
        public delegate void StateMachineOverride(BotState botState);

        public delegate void StateMachineStateChange();

        public delegate void StateMachineTick();

        public event StateMachineStateChange OnStateMachineStateChanged;

        public abstract event StateMachineTick OnStateMachineTick;

        public abstract event StateMachineOverride OnStateOverride;

        public KeyValuePair<BotState, BasicState> CurrentState { get; protected set; }

        public BotState LastState { get; protected set; }

        public Dictionary<BotState, BasicState> States { get; protected set; }

        public abstract void Execute();

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
    }
}