namespace AmeisenBotX.Core.Engines.Quest.Objects.Objectives
{
    public interface IQuestObjective
    {
        bool Finished { get; }

        double Progress { get; }

        void Execute();
    }
}