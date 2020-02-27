using AmeisenBotX.Core.Data;
using AmeisenBotX.Memory;

namespace AmeisenBotX.Core.StateMachine.States
{
    public class StateLoadingScreen : BasicState
    {
        public StateLoadingScreen(AmeisenBotStateMachine stateMachine, XMemory xMemory, AmeisenBotConfig config, ObjectManager objectManager) : base(stateMachine)
        {
            Config = config;
            ObjectManager = objectManager;
            XMemory = xMemory;
        }

        private AmeisenBotConfig Config { get; }

        private ObjectManager ObjectManager { get; }

        private XMemory XMemory { get; }

        public override void Enter()
        {
        }

        public override void Execute()
        {
            if (XMemory.Process != null && XMemory.Process.HasExited)
            {
                AmeisenBotStateMachine.SetState(BotState.None);
            }

            ObjectManager.RefreshIsWorldLoaded();
            if (ObjectManager.IsWorldLoaded)
            {
                AmeisenBotStateMachine.SetState(BotState.Idle);
            }
        }

        public override void Exit()
        {
        }
    }
}