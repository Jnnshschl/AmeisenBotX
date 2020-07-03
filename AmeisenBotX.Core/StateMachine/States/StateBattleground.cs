namespace AmeisenBotX.Core.Statemachine.States
{
    public class StateBattleground : BasicState
    {
        public StateBattleground(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, WowInterface wowInterface) : base(stateMachine, config, wowInterface)
        {
        }

        public override void Enter()
        {
            WowInterface.BattlegroundEngine?.Enter();
        }

        public override void Execute()
        {
            if (WowInterface.BattlegroundEngine == null)
            {
                return;
            }

            if (WowInterface.XMemory.Read(WowInterface.OffsetList.BattlegroundStatus, out int bgStatus)
                && bgStatus == 0)
            {
                StateMachine.SetState((int)BotState.Idle);
                return;
            }

            if (WowInterface.XMemory.Read(WowInterface.OffsetList.BattlegroundFinished, out int bgFinished)
                && bgFinished == 1)
            {
                WowInterface.HookManager.LeaveBattleground();
                return;
            }

            WowInterface.BattlegroundEngine.Execute();
        }

        public override void Exit()
        {
            WowInterface.BattlegroundEngine?.Exit();
            WowInterface.MovementEngine.Reset();
        }
    }
}