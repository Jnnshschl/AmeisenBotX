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
                StateMachine.SetState(BotState.StartWow);
            }
            else if (Config.AutoLogin)
            {
                StateMachine.SetState(BotState.Login);
            }
            else
            {
                StateMachine.SetState(BotState.Idle);
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