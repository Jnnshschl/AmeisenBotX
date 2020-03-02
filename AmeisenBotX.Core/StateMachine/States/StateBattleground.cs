namespace AmeisenBotX.Core.StateMachine.States
{
    public class StateBattleground : BasicState
    {
        public StateBattleground(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, WowInterface wowInterface) : base(stateMachine)
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
            if (WowInterface.XMemory.Read(WowInterface.OffsetList.BattlegroundStatus, out int bgStatus)
                && bgStatus == 0)
            {
                AmeisenBotStateMachine.SetState(BotState.Idle);
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
            WowInterface.MovementEngine.Reset();
        }
    }
}