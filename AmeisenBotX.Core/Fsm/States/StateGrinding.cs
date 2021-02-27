namespace AmeisenBotX.Core.Fsm.States
{
    public class StateGrinding : BasicState
    {
        public StateGrinding(AmeisenBotFsm stateMachine, AmeisenBotConfig config, WowInterface wowInterface) : base(stateMachine, config, wowInterface)
        {
        }

        public override void Enter()
        {
        }

        public override void Execute()
        {
            WowInterface.GrindingEngine.Execute();
        }

        public override void Leave()
        {
            WowInterface.GrindingEngine.Exit();
        }
    }
}