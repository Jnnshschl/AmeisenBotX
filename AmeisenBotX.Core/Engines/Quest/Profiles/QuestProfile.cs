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

        Queue<List<IBotQuest>> IQuestProfile.Quests => throw new NotImplementedException();

        public QuestProfile(AmeisenBotInterfaces bot)
        {
            BotInterfaces = bot;
        }
    }
}
