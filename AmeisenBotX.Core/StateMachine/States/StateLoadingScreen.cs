using AmeisenBotX.Core.Data;

namespace AmeisenBotX.Core.StateMachine.States
{
    public class StateLoadingScreen : State
    {
        public StateLoadingScreen(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, ObjectManager objectManager) : base(stateMachine)
        {
            Config = config;
            ObjectManager = objectManager;
        }

        private AmeisenBotConfig Config { get; }

        private ObjectManager ObjectManager { get; }

        public override void Enter()
        {
        }

        public override void Execute()
        {
            ObjectManager.RefreshIsWorldLoaded();
            if (ObjectManager.IsWorldLoaded)
                AmeisenBotStateMachine.SetState(AmeisenBotState.Idle);
        }

        public override void Exit()
        {
        }
    }
}