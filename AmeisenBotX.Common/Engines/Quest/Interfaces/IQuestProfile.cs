using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Quest.Objects.Quests
{
    public interface IQuestProfile
    {
        Queue<ICollection<IBotQuest>> Quests { get; }
    }
}