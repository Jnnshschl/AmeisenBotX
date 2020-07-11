using AmeisenBotX.Logging;

namespace AmeisenBotX.Core.Statemachine.States
{
    public class StateNone : BasicState
    {
        public StateNone(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, WowInterface wowInterface) : base(stateMachine, config, wowInterface)
        {
        }

        public override void Enter()
        {
            if (Config.AutostartWow)
            {
                AmeisenLogger.Instance.Log("StateNone", "Need to start WoW");
                StateMachine.SetState((int)BotState.StartWow);
            }
            else if (Config.AutoLogin)
            {
                StateMachine.SetState((int)BotState.Login);
            }
            else
            {
                StateMachine.SetState((int)BotState.Idle);
            }
        }

        public override void Execute()
        {
        }

        public override void Exit()
        {
        }
    }
}