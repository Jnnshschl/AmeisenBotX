using AmeisenBotX.Core.Engines.Quest.Objects.Objectives;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Quest.Objects.Quests
{
    public interface IBotQuest
    {
        bool Accepted { get; }

        bool Finished { get; }

        int Id { get; }

        string Name { get; }

        bool Returned { get; }

        List<IQuestObjective> Objectives { get; }

        void AcceptQuest();

        bool CompleteQuest();

        void Execute();
    }
}