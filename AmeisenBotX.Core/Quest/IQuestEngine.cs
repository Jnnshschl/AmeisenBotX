using AmeisenBotX.Core.Quest.Profiles;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Quest
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