using System.Collections.Generic;

namespace AmeisenBotX.Common.Engines.Quest.Interfaces
{
    public interface IBotQuest
    {
        bool Accepted { get; }

        bool Finished { get; }

        int Id { get; }

        string Name { get; }

        List<IQuestObjective> Objectives { get; }

        bool Returned { get; }

        void AcceptQuest();

        bool CompleteQuest();

        void Execute();
    }
}