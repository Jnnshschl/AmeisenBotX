namespace AmeisenBotX.Core.Statemachine.States
{
    public class StateDead : BasicState
    {
        public StateDead(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, WowInterface wowInterface) : base(stateMachine, config, wowInterface)
        {
        }

        public override void Enter()
        {
        }

        public override void Execute()
        {
            if (WowInterface.ObjectManager.Player.IsDead)
            {
                StateMachine.MapIDiedOn = WowInterface.ObjectManager.MapId;
                WowInterface.HookManager.ReleaseSpirit();
            }
            else if (WowInterface.HookManager.IsGhost("player"))
            {
                StateMachine.SetState(BotState.Ghost);
            }
            else
            {
                StateMachine.SetState(BotState.Idle);
            }
        }

        public override void Exit()
        {
        }
    }
}