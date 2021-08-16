using AmeisenBotX.Core.Logic.Enums;
using AmeisenBotX.Logging;

namespace AmeisenBotX.Core.Logic.States
{
    public class StateNone : BasicState
    {
        public StateNone(AmeisenBotFsm stateMachine, AmeisenBotConfig config, AmeisenBotInterfaces bot) : base(stateMachine, config, bot)
        {
        }

        public override void Enter()
        {
            if (Config.AutostartWow)
            {
                AmeisenLogger.I.Log("StateNone", "Need to start WoW");
                StateMachine.SetState(BotState.StartWow);
            }
        }

        public override void Execute()
        {
        }

        public override void Leave()
        {
        }
    }
}