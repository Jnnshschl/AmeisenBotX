using AmeisenBotX.Core.Engines.Quest.Profiles;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Quest
{
    public interface IQuestEngine
    {
        List<int> CompletedQuests { get; }

        IQuestProfile SelectedProfile { get; set; }

        bool UpdatedCompletedQuests { get; set; }

        void Enter();

        void Execute();
    }
}