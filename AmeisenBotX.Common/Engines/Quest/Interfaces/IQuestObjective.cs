namespace AmeisenBotX.Common.Engines.Quest.Interfaces
{
    public interface IQuestObjective
    {
        bool Finished { get; }

        double Progress { get; }

        void Execute();
    }
}