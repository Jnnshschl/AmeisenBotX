using System.Collections.Generic;

namespace AmeisenBotX.Common.Engines.Quest.Interfaces
{
    public interface IQuestProfile
    {
        Queue<List<IBotQuest>> Quests { get; }
    }
}