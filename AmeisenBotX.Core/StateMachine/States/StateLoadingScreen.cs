namespace AmeisenBotX.Core.Statemachine.States
{
    public class StateLoadingScreen : BasicState
    {
        public StateLoadingScreen(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, WowInterface wowInterface) : base(stateMachine, config, wowInterface)
        {
        }

        public override void Enter()
        {
        }

        public override void Execute()
        {
            if (WowInterface.XMemory.Process == null || WowInterface.WowProcess.HasExited)
            {
                StateMachine.SetState(BotState.None);
            }

            WowInterface.ObjectManager.RefreshIsWorldLoaded();
            if (WowInterface.ObjectManager.IsWorldLoaded)
            {
                StateMachine.SetState(BotState.Idle);
            }
        }

        public override void Exit()
        {
        }
    }
}