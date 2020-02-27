namespace AmeisenBotX.Core.StateMachine.States
{
    public abstract class BasicState
    {
        public BasicState(AmeisenBotStateMachine stateMachine)
        {
            AmeisenBotStateMachine = stateMachine;
        }

        internal AmeisenBotStateMachine AmeisenBotStateMachine { get; }

        public abstract void Enter();

        public abstract void Execute();

        public abstract void Exit();
    }
}