using AmeisenBotX.Core.Quest.Objects.Quests;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Quest.Profiles
{
    public interface IQuestProfile
    {
        Queue<List<BotQuest>> Quests { get; }
    }
}