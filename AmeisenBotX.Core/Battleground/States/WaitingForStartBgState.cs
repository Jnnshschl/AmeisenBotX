namespace AmeisenBotX.Core.Battleground.States
{
    public class WaitingForStartBgState : BasicBattlegroundState
    {
        public WaitingForStartBgState(BattlegroundEngine battlegroundEngine) : base(battlegroundEngine)
        {
        }

        public override void Enter()
        {
        }

        public override void Execute()
        {
            // TODO: recognize wether the BG is running or not
            if (true)
            {
                BattlegroundEngine.SetState(BattlegroundState.DefendMyself);
            }
        }

        public override void Exit()
        {
        }
    }
}