using AmeisenBotX.Core.Engines.Quest.Objects.Quests;
using AmeisenBotX.Core.Engines.Quest.Quests.Grinder;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Quest.Profiles.Shino
{
    internal class Horde1To60GrinderProfile : IQuestProfile
    {
        public Horde1To60GrinderProfile(AmeisenBotInterfaces bot)
        {
            Quests = new Queue<List<IBotQuest>>();
            Quests.Enqueue([
                new QDurotarGrindToLevel6(bot)
            ]);
            Quests.Enqueue([
                new QDurotarGrindToLevel9(bot)
            ]);
            Quests.Enqueue([
                new QDurotarGrindToLevel11(bot)
            ]);
            Quests.Enqueue([
                new QTheBarrensGrindToLevel14(bot)
            ]);
            Quests.Enqueue([
                new QTheBarrensGrindToLevel16(bot)
            ]);
            Quests.Enqueue([
                new QTheBarrensGrindToLevel19(bot)
            ]);
            Quests.Enqueue([
                new QStonetalonGrindToLevel23(bot)
            ]);
            Quests.Enqueue([
                new QStonetalonGrindToLevel31(bot)
            ]);
            Quests.Enqueue([
                new QDesolaceGrindToLevel35(bot)
            ]);
            Quests.Enqueue([
                new QDesolaceGrindToLevel40(bot)
            ]);
            Quests.Enqueue([
                new QTanarisGrindToLevel44(bot)
            ]);
            Quests.Enqueue([
                new QTanarisGrindToLevel49(bot)
            ]);
            Quests.Enqueue([
                new QUngoroGrindToLevel54(bot)
            ]);
            Quests.Enqueue([
                new QSilithusGrindToLevel60(bot)
            ]);
        }

        public Queue<List<IBotQuest>> Quests { get; }

        public override string ToString()
        {
            return $"[1-60] Horde1To60GrinderProfile (Shino)";
        }
    }
}