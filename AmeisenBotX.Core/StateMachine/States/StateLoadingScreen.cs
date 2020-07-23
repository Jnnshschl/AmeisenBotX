using AmeisenBotX.Logging;

namespace AmeisenBotX.Core.Statemachine.States
{
    public class StateLoadingScreen : BasicState
    {
        public StateLoadingScreen(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, WowInterface wowInterface) : base(stateMachine, config, wowInterface)
        {
        }

        public override void Enter()
        {
            AmeisenLogger.Instance.Log("LoadingScreen", "Entered loading screen");
        }

        public override void Execute()
        {
            if (WowInterface.XMemory.Process == null || WowInterface.XMemory.Process.HasExited)
            {
                AmeisenLogger.Instance.Log("LoadingScreen", "WowProcess exited");
                StateMachine.SetState(BotState.None);
                return;
            }

            // if (WowInterface.XMemory.ReadString(WowInterface.OffsetList.GameState, Encoding.ASCII, out string gameState)
            //     && gameState.Contains("login") || gameState.Contains("charselect"))
            // {
            //     StateMachine.SetState(BotState.Login);
            //     return;
            // }

            WowInterface.ObjectManager.RefreshIsWorldLoaded();

            if (WowInterface.ObjectManager.IsWorldLoaded)
            {
                StateMachine.SetState(BotState.Idle);
                return;
            }
        }

        public override void Exit()
        {
            AmeisenLogger.Instance.Log("LoadingScreen", "Exited loading screen");
            WowInterface.MovementEngine.StopMovement();
        }
    }
}