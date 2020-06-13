using AmeisenBotX.Core.Common;

namespace AmeisenBotX.Core.Movement.SMovementEngine.States
{
    public abstract class BasicMovementState : IState
    {
        public BasicMovementState(StateBasedMovementEngine stateMachine, AmeisenBotConfig config, WowInterface wowInterface)
        {
            StateMachine = stateMachine;
            Config = config;
            WowInterface = wowInterface;
        }

        internal AmeisenBotConfig Config { get; }

        internal StateBasedMovementEngine StateMachine { get; }

        internal WowInterface WowInterface { get; }

        public abstract void Enter();

        public abstract void Execute();

        public abstract void Exit();
    }
}