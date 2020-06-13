using AmeisenBotX.Core.Common;
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
        }

        public override void Execute()
        {
            if (WowInterface.XMemory.Process == null || WowInterface.WowProcess.HasExited)
            {
                StateMachine.SetState((int)BotState.None);
                return;
            }

            if (WowInterface.XMemory.ReadString(WowInterface.OffsetList.GameState, Encoding.ASCII, out string gameState)
                && gameState.Contains("login"))
            {
                StateMachine.SetState((int)BotState.Login);
                BotUtils.SendKey(WowInterface.XMemory.Process.MainWindowHandle, new IntPtr(0x1B));
                return;
            }

            WowInterface.ObjectManager.RefreshIsWorldLoaded();
            if (WowInterface.ObjectManager.IsWorldLoaded)
            {
                StateMachine.SetState((int)BotState.Idle);
                return;
            }
        }

        public override void Exit()
        {
        }
    }
}