namespace AmeisenBotX.Common.Engines.Battleground.Interfaces
{
    public interface IBattlegroundEngine
    {
        string Author { get; }

        string Description { get; }

        string Name { get; }

        void Execute();

        void Reset();
    }
}