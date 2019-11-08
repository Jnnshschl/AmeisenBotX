namespace AmeisenBotX.Core.StateMachine.States
{
    public class StateNone : State
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
                AmeisenBotStateMachine.SetState(AmeisenBotState.StartWow);
            }
            else if (Config.AutoLogin)
            {
                AmeisenBotStateMachine.SetState(AmeisenBotState.Login);
            }
            else
            {
                AmeisenBotStateMachine.SetState(AmeisenBotState.Idle);
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
