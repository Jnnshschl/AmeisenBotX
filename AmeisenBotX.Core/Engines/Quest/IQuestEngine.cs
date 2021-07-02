using AmeisenBotX.Core.Engines.Quest.Profiles;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Quest
{
    public interface IQuestEngine
    {
        List<int> CompletedQuests { get; }

        IQuestProfile Profile { get; set; }

        bool UpdatedCompletedQuests { get; set; }

        void Execute();

        void Start();
    }
}