namespace AmeisenBotX.Core.Statemachine.States
{
    public class StateJob : BasicState
    {
        public StateJob(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, WowInterface wowInterface) : base(stateMachine, config, wowInterface)
        {
        }

        public override void Enter()
        {
            WowInterface.JobEngine.Enter();
            WowInterface.Globals.IgnoreMountDistance = true;
        }

        public override void Execute()
        {
            WowInterface.JobEngine.Execute();
        }

        public override void Exit()
        {
            WowInterface.Globals.IgnoreMountDistance = false;
            WowInterface.JobEngine.Reset();
        }
    }
}