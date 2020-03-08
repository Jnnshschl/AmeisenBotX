namespace AmeisenBotX.Core.Statemachine.States
{
    public class StateDungeon : BasicState
    {
        public StateDungeon(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, WowInterface wowInterface) : base(stateMachine, config, wowInterface)
        {
        }

        public override void Enter()
        {
        }

        public override void Execute()
        {
            if (StateMachine.IsInDungeon())
            {
                WowInterface.DungeonEngine.Execute();
            }
            else
            {
                StateMachine.SetState(BotState.Idle);
            }
        }

        public override void Exit()
        {
            WowInterface.DungeonEngine.Reset();
        }
    }
}