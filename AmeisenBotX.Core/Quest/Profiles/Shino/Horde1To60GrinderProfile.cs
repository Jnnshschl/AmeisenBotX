using AmeisenBotX.Core.Quest.Objects.Quests;
using System.Collections.Generic;
using AmeisenBotX.Core.Quest.Quests.Grinder;

namespace AmeisenBotX.Core.Quest.Profiles.Shino
{
    class Horde1To60GrinderProfile : IQuestProfile
    {
        public Queue<List<IBotQuest>> Quests { get; }

        public Horde1To60GrinderProfile(WowInterface wowInterface)
        {
            Quests = new Queue<List<IBotQuest>>();
            Quests.Enqueue(new List<IBotQuest>() {
                new QDurotarGrindToLevel6(wowInterface)
            });
            Quests.Enqueue(new List<IBotQuest>() {
                new QDurotarGrindToLevel9(wowInterface)
            });
            Quests.Enqueue(new List<IBotQuest>() {
                new QDurotarGrindToLevel11(wowInterface)
            });
            Quests.Enqueue(new List<IBotQuest>() {
                new QTheBarrensGrindToLevel14(wowInterface)
            });
            Quests.Enqueue(new List<IBotQuest>() {
                new QTheBarrensGrindToLevel16(wowInterface)
            });
            Quests.Enqueue(new List<IBotQuest>() {
                new QTheBarrensGrindToLevel19(wowInterface)
            });
            Quests.Enqueue(new List<IBotQuest>() {
                new QStonetalonGrindToLevel23(wowInterface)
            });
            Quests.Enqueue(new List<IBotQuest>() {
                new QStonetalonGrindToLevel31(wowInterface)
            });
            Quests.Enqueue(new List<IBotQuest>() {
                new QDesolaceGrindToLevel35(wowInterface)
            });
            Quests.Enqueue(new List<IBotQuest>() {
                new QDesolaceGrindToLevel40(wowInterface)
            });
            Quests.Enqueue(new List<IBotQuest>() {
                new QTanarisGrindToLevel44(wowInterface)
            });
            Quests.Enqueue(new List<IBotQuest>() {
                new QTanarisGrindToLevel49(wowInterface)
            });
            Quests.Enqueue(new List<IBotQuest>() {
                new QUngoroGrindToLevel54(wowInterface)
            });
            Quests.Enqueue(new List<IBotQuest>() {
                new QSilithusGrindToLevel60(wowInterface)
            });
        }
        public override string ToString()
        {
            return $"[1-60] Horde1To60GrinderProfile (Shino)";
        }
    }
}
