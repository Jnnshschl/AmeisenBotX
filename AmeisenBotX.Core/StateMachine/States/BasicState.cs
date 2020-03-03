namespace AmeisenBotX.Core.Statemachine.States
{
    public abstract class BasicState
    {
        public BasicState(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, WowInterface wowInterface)
        {
            StateMachine = stateMachine;
            Config = config;
            WowInterface = wowInterface;
        }

        internal AmeisenBotConfig Config { get; }

        internal AmeisenBotStateMachine StateMachine { get; }

        internal WowInterface WowInterface { get; }

        public abstract void Enter();

        public abstract void Execute();

        public abstract void Exit();
    }
}