using AmeisenBotX.Core.Engines.Quest.Objects.Quests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.Engines.Quest.Profiles
{
    public abstract class QuestProfile : IQuestProfile
    {
        protected AmeisenBotInterfaces BotInterfaces { get; set; }

        Queue<ICollection<IBotQuest>> IQuestProfile.Quests { get; } = new Queue<ICollection<IBotQuest>>();

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
