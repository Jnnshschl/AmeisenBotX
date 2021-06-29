using AmeisenBotX.Core.Fsm.Enums;

namespace AmeisenBotX.Core.Fsm.States
{
    public class StateBattleground : BasicState
    {
        public StateBattleground(AmeisenBotFsm stateMachine, AmeisenBotConfig config, WowInterface wowInterface) : base(stateMachine, config, wowInterface)
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
                StateMachine.SetState(BotState.Idle);
                return;
            }

            if (WowInterface.XMemory.Read(WowInterface.OffsetList.BattlegroundFinished, out int bgFinished)
                && bgFinished == 1)
            {
                WowInterface.NewWowInterface.LuaLeaveBattleground();
                return;
            }

            WowInterface.BattlegroundEngine.Execute();
        }

        public override void Leave()
        {
            WowInterface.BattlegroundEngine?.Leave();
            WowInterface.MovementEngine.Reset();
        }
    }
}