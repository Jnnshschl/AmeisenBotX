namespace AmeisenBotX.Core.StateMachine.States
{
    public class StateJob : BasicState
    {
        public StateJob(AmeisenBotStateMachine stateMachine, WowInterface wowInterface) : base(stateMachine)
        {
            WowInterface = wowInterface;
        }

        private WowInterface WowInterface { get; }

        public override void Enter()
        {
        }

        public override void Execute()
        {
            WowInterface.JobEngine.Execute();
        }

        public override void Exit()
        {
            WowInterface.JobEngine.Reset();
        }
    }
}