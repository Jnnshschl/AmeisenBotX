namespace AmeisenBotX.Core.Statemachine.States
{
    public class StateGrinding : BasicState
    {
        public StateGrinding(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, WowInterface wowInterface) : base(stateMachine, config, wowInterface)
        {
        }

        public override void Enter()
        {
        }

        public override void Execute()
        {
            WowInterface.GrindingEngine.Execute();
        }

        public override void Exit()
        {
            WowInterface.GrindingEngine.Exit();
        }
    }
}