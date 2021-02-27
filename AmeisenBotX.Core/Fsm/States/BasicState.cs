using AmeisenBotX.Statemachine.States;

namespace AmeisenBotX.Core.Fsm.States
{
    public abstract class BasicState : IState
    {
        public BasicState(AmeisenBotFsm stateMachine, AmeisenBotConfig config, WowInterface wowInterface)
        {
            StateMachine = stateMachine;
            Config = config;
            WowInterface = wowInterface;
        }

        internal AmeisenBotConfig Config { get; }

        internal AmeisenBotFsm StateMachine { get; }

        internal WowInterface WowInterface { get; }

        public abstract void Enter();

        public abstract void Execute();

        public abstract void Leave();
    }
}