using AmeisenBotX.Core.Engines.Quest.Objects.Quests;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Quest.Profiles
{
    public abstract class QuestProfile : IQuestProfile
    {
        protected AmeisenBotInterfaces BotInterfaces { get; set; }

        public Queue<ICollection<IBotQuest>> Quests { get; } = new Queue<ICollection<IBotQuest>>();

        public QuestProfile(AmeisenBotInterfaces bot)
        {
            BotInterfaces = bot;
        }

        public string Name { get; init; }

        public override string ToString()
        {
            return Name;
        }
    }
}
