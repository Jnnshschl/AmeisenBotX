namespace AmeisenBotX.Core.StateMachine.States
{
    public class StateLoadingScreen : BasicState
    {
        public StateLoadingScreen(AmeisenBotStateMachine stateMachine, WowInterface wowInterface) : base(stateMachine)
        {
            WowInterface = wowInterface;
        }

        private WowInterface WowInterface { get; }

        public override void Enter()
        {
        }

        public override void Execute()
        {
            if (WowInterface.XMemory.Process != null && WowInterface.WowProcess.HasExited)
            {
                AmeisenBotStateMachine.SetState(BotState.None);
            }

            WowInterface.ObjectManager.RefreshIsWorldLoaded();
            if (WowInterface.ObjectManager.IsWorldLoaded)
            {
                AmeisenBotStateMachine.SetState(BotState.Idle);
            }
        }

        public override void Exit()
        {
        }
    }
}