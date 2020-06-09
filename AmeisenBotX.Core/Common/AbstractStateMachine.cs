using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Common
{
    public abstract class AbstractStateMachine<T> where T : IState
    {
        public delegate void StateMachineOverride(int botState);

        public delegate void StateMachineStateChange();

        public delegate void StateMachineTick();

        public event StateMachineStateChange OnStateMachineStateChanged;

        public abstract event StateMachineTick OnStateMachineTick;

        public abstract event StateMachineOverride OnStateOverride;

        public KeyValuePair<int, T> CurrentState { get; protected set; }

        public int LastState { get; protected set; }

        public Dictionary<int, T> States { get; protected set; }

        public abstract void Execute();

        public bool SetState(int state, bool ignoreExit = false)
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