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
            StateMachine.OnStateOverride += StateMachine_OnStateOverride;
        }

        public override void Execute()
        {
            if (!StateMachine.IsDungeonMap(WowInterface.ObjectManager.MapId))
            {
                StateMachine.SetState((int)BotState.Idle);
                return;
            }

            WowInterface.DungeonEngine.Execute();
            WowInterface.CombatClass?.OutOfCombatExecute();
        }

        public override void Exit()
        {
            StateMachine.OnStateOverride -= StateMachine_OnStateOverride;
            WowInterface.MovementEngine.Reset();
            WowInterface.DungeonEngine.Reset();
        }

        private void StateMachine_OnStateOverride(int botState)
        {
            if (botState == (int)BotState.Dead)
            {
                WowInterface.DungeonEngine.OnDeath();
            }
        }
    }
}