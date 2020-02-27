namespace AmeisenBotX.Core.StateMachine.States
{
    public class StateNone : BasicState
    {
        public StateNone(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config) : base(stateMachine)
        {
            Config = config;
        }

        private AmeisenBotConfig Config { get; }

        public override void Enter()
        {
            if (Config.AutostartWow)
            {
                AmeisenBotStateMachine.SetState(BotState.StartWow);
            }
            else if (Config.AutoLogin)
            {
                AmeisenBotStateMachine.SetState(BotState.Login);
            }
            else
            {
                AmeisenBotStateMachine.SetState(BotState.Idle);
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