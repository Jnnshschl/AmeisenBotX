namespace AmeisenBotX.Core.Fsm.States
{
    public class StateJob : BasicState
    {
        public StateJob(AmeisenBotFsm stateMachine, AmeisenBotConfig config, WowInterface wowInterface) : base(stateMachine, config, wowInterface)
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

        public override void Leave()
        {
            WowInterface.Globals.IgnoreMountDistance = false;
            WowInterface.JobEngine.Reset();
        }
    }
}