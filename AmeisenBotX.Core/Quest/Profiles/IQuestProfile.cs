using AmeisenBotX.Core.Quest.Objects.Quests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.Quest.Profiles
{
    public interface IQuestProfile
    {
        Queue<List<BotQuest>> Quests { get; }
    }
}
