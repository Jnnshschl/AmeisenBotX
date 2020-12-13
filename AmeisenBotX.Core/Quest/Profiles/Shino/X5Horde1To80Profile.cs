using AmeisenBotX.Core.Quest.Objects.Quests;
using AmeisenBotX.Core.Quest.Quests.StartArea;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Quest.Profiles.Shino
{
    class X5Horde1To80Profile : IQuestProfile
    {
        public Queue<List<BotQuest>> Quests { get; }

        public X5Horde1To80Profile(WowInterface wowInterface)
        {
            var valleyOfStrength = new ValleyOfStrength(wowInterface);

            Quests = new Queue<List<BotQuest>>();
            Quests.Enqueue(new List<BotQuest>() {
                valleyOfStrength.QCuttingTeeth
            });
        }
    }
}
