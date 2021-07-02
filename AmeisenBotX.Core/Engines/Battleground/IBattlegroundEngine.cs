namespace AmeisenBotX.Core.Engines.Battleground
{
    public interface IBattlegroundEngine
    {
        string Author { get; }

        string Description { get; }

        string Name { get; }

        void Enter();

        void Execute();

        void Leave();
    }
}