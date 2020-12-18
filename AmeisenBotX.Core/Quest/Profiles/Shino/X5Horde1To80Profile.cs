using AmeisenBotX.Core.Quest.Objects.Quests;
using System.Collections.Generic;
using AmeisenBotX.Core.Quest.Quests.StartArea.ValleyOfStrength;

namespace AmeisenBotX.Core.Quest.Profiles.Shino
{
    class X5Horde1To80Profile : IQuestProfile
    {
        public Queue<List<BotQuest>> Quests { get; }

        public X5Horde1To80Profile(WowInterface wowInterface)
        {
            Quests = new Queue<List<BotQuest>>();
            Quests.Enqueue(new List<BotQuest>() {
                new QYourPlaceInTheWorld(wowInterface)
            });
            Quests.Enqueue(new List<BotQuest>() {
                new QCuttingTeeth(wowInterface)
            });
            Quests.Enqueue(new List<BotQuest>() {
                new QStingOfTheScorpid(wowInterface),
                new QGalgarCactusAppleSurprise(wowInterface)
            });
            Quests.Enqueue(new List<BotQuest>() {
                new QSarkoth(wowInterface)
            });
            Quests.Enqueue(new List<BotQuest>() {
                new QSarkoth2(wowInterface)
            });
        }
        public override string ToString()
        {
            return $"[1-80] X5Horde1To80Profile (Shino)";
        }
    }
}
