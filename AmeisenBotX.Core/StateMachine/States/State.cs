namespace AmeisenBotX.Core.StateMachine.States
{
    public abstract class State
    {
        internal AmeisenBotStateMachine AmeisenBotStateMachine { get; }

        public State(AmeisenBotStateMachine stateMachine)
        {
            AmeisenBotStateMachine = stateMachine;
        }

        public abstract void Enter();

        public abstract void Execute();

        public abstract void Exit();
    }
}