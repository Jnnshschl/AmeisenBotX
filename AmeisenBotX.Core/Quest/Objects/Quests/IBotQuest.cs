namespace AmeisenBotX.Core.Quest.Objects.Quests
{
    public interface IBotQuest
    {
        bool Accepted { get; }

        bool Finished { get; }

        int Id { get; }

        string Name { get; }

        bool Returned { get; }

        void AcceptQuest();

        bool CompleteQuest();

        void Execute();
    }
}