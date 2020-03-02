namespace AmeisenBotX.Core.StateMachine.States
{
    public class StateDead : BasicState
    {
        public StateDead(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, WowInterface wowInterface) : base(stateMachine)
        {
            Config = config;
            WowInterface = wowInterface;
        }

        private AmeisenBotConfig Config { get; }

        private WowInterface WowInterface { get; }

        public override void Enter()
        {
        }

        public override void Execute()
        {
            if (WowInterface.ObjectManager.Player.IsDead)
            {
                WowInterface.HookManager.ReleaseSpirit();
            }
            else if (WowInterface.HookManager.IsGhost("player"))
            {
                AmeisenBotStateMachine.SetState(BotState.Ghost);
            }
            else
            {
                AmeisenBotStateMachine.SetState(BotState.Idle);
            }
        }

        public override void Exit()
        {
        }
    }
}