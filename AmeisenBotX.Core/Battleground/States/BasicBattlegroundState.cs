namespace AmeisenBotX.Core.Battleground.States
{
    public abstract class BasicBattlegroundState
    {
        public BasicBattlegroundState(BattlegroundEngine battlegroundEngine)
        {
            BattlegroundEngine = battlegroundEngine;
        }

        internal BattlegroundEngine BattlegroundEngine { get; }

        public abstract void Enter();

        public abstract void Execute();

        public abstract void Exit();
    }
}