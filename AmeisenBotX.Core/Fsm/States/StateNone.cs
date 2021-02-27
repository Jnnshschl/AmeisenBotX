using AmeisenBotX.Core.Fsm.Enums;
using AmeisenBotX.Logging;

namespace AmeisenBotX.Core.Fsm.States
{
    public class StateNone : BasicState
    {
        public StateNone(AmeisenBotFsm stateMachine, AmeisenBotConfig config, WowInterface wowInterface) : base(stateMachine, config, wowInterface)
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