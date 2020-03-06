namespace AmeisenBotX.Core.Statemachine.States
{
    public class StateRelaxing : BasicState
    {
        public StateRelaxing(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, WowInterface wowInterface) : base(stateMachine, config, wowInterface)
        {
        }

        public override void Enter()
        {
        }

        public override void Execute()
        {
            // if we are not in a group and in a relaxing zone
            if (WowInterface.ObjectManager.PartyleaderGuid == 0 && StateMachine.IsInCapitalCity())
            {
                WowInterface.RelaxEngine.Execute();
            }
            else
            {
                StateMachine.SetState(BotState.Idle);
            }
        }

        public override void Exit()
        {
            WowInterface.DungeonEngine.Reset();
        }
    }
}