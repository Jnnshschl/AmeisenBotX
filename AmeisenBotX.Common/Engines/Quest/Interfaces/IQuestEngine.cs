using System.Collections.Generic;

namespace AmeisenBotX.Common.Engines.Quest.Interfaces
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