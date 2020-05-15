namespace AmeisenBotX.Core.Statemachine.States
{
    public class StateDungeon : BasicState
    {
        public StateDungeon(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, WowInterface wowInterface) : base(stateMachine, config, wowInterface)
        {
        }

        public override void Enter()
        {
            WowInterface.MovementEngine.Reset();
        }

        public override void Execute()
        {
            if (StateMachine.IsDungeonMap(WowInterface.ObjectManager.MapId))
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