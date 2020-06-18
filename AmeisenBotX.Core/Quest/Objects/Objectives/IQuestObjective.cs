namespace AmeisenBotX.Core.Quest.Objects.Objectives
{
    public interface IQuestObjective
    {
        bool Finished { get; }

        double Progress { get; }

        void Execute();
    }
}