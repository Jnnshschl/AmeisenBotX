namespace AmeisenBotX.Core.StateMachine.States
{
    public class StateEating : BasicState
    {
        public StateEating(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, WowInterface wowInterface) : base(stateMachine)
        {
            Config = config;
            WowInterface = wowInterface;
        }

        private AmeisenBotConfig Config { get; }

        private WowInterface WowInterface { get; }

        public override void Enter()
        {
        }

        public override void Execute()
        {
        }

        public override void Exit()
        {
        }
    }
}