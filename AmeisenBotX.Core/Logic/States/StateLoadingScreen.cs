using AmeisenBotX.Core.Logic.Enums;
using AmeisenBotX.Logging;
using System;
using System.Text;

namespace AmeisenBotX.Core.Logic.States
{
    public class StateLoadingScreen : BasicState
    {
        public StateLoadingScreen(AmeisenBotFsm stateMachine, AmeisenBotConfig config, AmeisenBotInterfaces bot) : base(stateMachine, config, bot)
        {
        }

        public override void Enter()
        {
            AmeisenLogger.I.Log("LoadingScreen", "Entered loading screen");
        }

        public override void Execute()
        {
            if (Bot.Memory.Process == null || Bot.Memory.Process.HasExited)
            {
                AmeisenLogger.I.Log("LoadingScreen", "WowProcess exited");
                StateMachine.SetState(BotState.None);
            }
            else if (Bot.Memory.ReadString(Bot.Wow.Offsets.GameState, Encoding.ASCII, out string glueFrame)
                    && glueFrame.Equals("login", StringComparison.OrdinalIgnoreCase))
            {
                AmeisenLogger.I.Log("LoadingScreen", "Returned to login screen");
                StateMachine.SetState(BotState.Login);
            }
            else if (Bot.Objects.IsWorldLoaded)
            {
                StateMachine.SetState(BotState.Idle);
            }
        }

        public override void Leave()
        {
            AmeisenLogger.I.Log("LoadingScreen", "Exited loading screen");
            Bot.Movement.StopMovement();
        }
    }
}