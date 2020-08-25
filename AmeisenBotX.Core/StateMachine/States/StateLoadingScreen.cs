using AmeisenBotX.Logging;
using System;
using System.Text;

namespace AmeisenBotX.Core.Statemachine.States
{
    public class StateLoadingScreen : BasicState
    {
        public StateLoadingScreen(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, WowInterface wowInterface) : base(stateMachine, config, wowInterface)
        {
        }

        public override void Enter()
        {
            AmeisenLogger.I.Log("LoadingScreen", "Entered loading screen");
            WowInterface.CombatClass.PriorityTargets = null;
        }

        public override void Execute()
        {
            if (WowInterface.XMemory.Process == null || WowInterface.XMemory.Process.HasExited)
            {
                AmeisenLogger.I.Log("LoadingScreen", "WowProcess exited");
                StateMachine.SetState(BotState.None);
                return;
            }

            if (WowInterface.XMemory.ReadString(WowInterface.OffsetList.GameState, Encoding.ASCII, out string glueFrame)
                && glueFrame.Equals("login", StringComparison.OrdinalIgnoreCase))
            {
                AmeisenLogger.I.Log("LoadingScreen", "Returned to login screen");
                StateMachine.SetState(BotState.Login);
                return;
            }

            WowInterface.ObjectManager.RefreshIsWorldLoaded();

            if (WowInterface.ObjectManager.IsWorldLoaded)
            {
                StateMachine.SetState(BotState.Idle);
                return;
            }
        }

        public override void Leave()
        {
            AmeisenLogger.I.Log("LoadingScreen", "Exited loading screen");
            WowInterface.MovementEngine.StopMovement();
        }
    }
}