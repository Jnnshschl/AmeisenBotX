using AmeisenBotX.Core.Quest.Objects.Quests;
using System.Collections.Generic;
using AmeisenBotX.Core.Quest.Quests.Durotar.RazorHill;
using AmeisenBotX.Core.Quest.Quests.Durotar.ValleyOfStrength;

namespace AmeisenBotX.Core.Quest.Profiles.Shino
{
    class X5Horde1To80Profile : IQuestProfile
    {
        public Queue<List<IBotQuest>> Quests { get; }

        public X5Horde1To80Profile(WowInterface wowInterface)
        {
            Quests = new Queue<List<IBotQuest>>();
            Quests.Enqueue(new List<IBotQuest>() {
                new QYourPlaceInTheWorld(wowInterface)
            });
            Quests.Enqueue(new List<IBotQuest>() {
                new QCuttingTeeth(wowInterface)
            });
            Quests.Enqueue(new List<IBotQuest>() {
                new QStingOfTheScorpid(wowInterface),
                new QVileFamiliars(wowInterface),
                new QGalgarCactusAppleSurprise(wowInterface)
            });
            Quests.Enqueue(new List<IBotQuest>() {
                new QSarkoth(wowInterface)
            });
            Quests.Enqueue(new List<IBotQuest>() {
                new QSarkoth2(wowInterface)
            });
            Quests.Enqueue(new List<IBotQuest>() {
                new QAPeonBurden(wowInterface)
            });
            Quests.Enqueue(new List<IBotQuest>() {
                new QVanquishTheBetrayers(wowInterface),
                new QEncroachment(wowInterface)
            });
        }
        public override string ToString()
        {
            return $"[1-80] X5Horde1To80Profile (Shino)";
        }
    }
}
