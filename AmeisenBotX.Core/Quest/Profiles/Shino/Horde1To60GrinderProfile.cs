using AmeisenBotX.Core.Quest.Objects.Quests;
using AmeisenBotX.Core.Quest.Quests.Grinder;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Quest.Profiles.Shino
{
    internal class Horde1To60GrinderProfile : IQuestProfile
    {
        public Horde1To60GrinderProfile(AmeisenBotInterfaces bot)
        {
            Quests = new Queue<List<IBotQuest>>();
            Quests.Enqueue(new List<IBotQuest>() {
                new QDurotarGrindToLevel6(bot)
            });
            Quests.Enqueue(new List<IBotQuest>() {
                new QDurotarGrindToLevel9(bot)
            });
            Quests.Enqueue(new List<IBotQuest>() {
                new QDurotarGrindToLevel11(bot)
            });
            Quests.Enqueue(new List<IBotQuest>() {
                new QTheBarrensGrindToLevel14(bot)
            });
            Quests.Enqueue(new List<IBotQuest>() {
                new QTheBarrensGrindToLevel16(bot)
            });
            Quests.Enqueue(new List<IBotQuest>() {
                new QTheBarrensGrindToLevel19(bot)
            });
            Quests.Enqueue(new List<IBotQuest>() {
                new QStonetalonGrindToLevel23(bot)
            });
            Quests.Enqueue(new List<IBotQuest>() {
                new QStonetalonGrindToLevel31(bot)
            });
            Quests.Enqueue(new List<IBotQuest>() {
                new QDesolaceGrindToLevel35(bot)
            });
            Quests.Enqueue(new List<IBotQuest>() {
                new QDesolaceGrindToLevel40(bot)
            });
            Quests.Enqueue(new List<IBotQuest>() {
                new QTanarisGrindToLevel44(bot)
            });
            Quests.Enqueue(new List<IBotQuest>() {
                new QTanarisGrindToLevel49(bot)
            });
            Quests.Enqueue(new List<IBotQuest>() {
                new QUngoroGrindToLevel54(bot)
            });
            Quests.Enqueue(new List<IBotQuest>() {
                new QSilithusGrindToLevel60(bot)
            });
        }

        public Queue<List<IBotQuest>> Quests { get; }

        public override string ToString()
        {
            return $"[1-60] Horde1To60GrinderProfile (Shino)";
        }
    }
}