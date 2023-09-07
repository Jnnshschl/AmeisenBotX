using AmeisenBotX.Common;
using System.Collections.Generic;

namespace AmeisenBotX.Common.Engines.Quest.Interfaces
{
    public interface IQuestEngine
    {
        AmeisenBotInterfaces Bot { get; set; }

        List<int> CompletedQuests { get; }

        IQuestProfile SelectedProfile { get; set; }

        bool UpdatedCompletedQuests { get; set; }
        ICollection<IQuestProfile> Profiles { get; }

        void Enter();

        void Execute();
    }
}