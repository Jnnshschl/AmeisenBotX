using AmeisenBotX.Core.Engines.Quest.Objects.Quests;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Quest.Profiles
{
    public interface IQuestProfile
    {
        Queue<List<IBotQuest>> Quests { get; }
    }
}