using AmeisenBotX.Core.Engines.Quest.Objects.Quests;
using AmeisenBotX.Core.Engines.Quest.Quests.Durotar.RazorHill;
using AmeisenBotX.Core.Engines.Quest.Quests.Durotar.ValleyOfStrength;
using AmeisenBotX.Core.Engines.Quest.Quests.TheBarrens.Crossroads;
using AmeisenBotX.Core.Engines.Quest.Quests.TheBarrens.OutpostBridge;
using AmeisenBotX.Core.Engines.Quest.Quests.TheBarrens.OutpostStonetalon;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Quest.Profiles.Shino
{
    internal class X5Horde1To80Profile : IQuestProfile
    {
        public X5Horde1To80Profile(AmeisenBotInterfaces bot)
        {
            Quests = new Queue<List<IBotQuest>>();
            Quests.Enqueue([
                new QYourPlaceInTheWorld(bot)
            ]);
            Quests.Enqueue([
                new QCuttingTeeth(bot)
            ]);
            Quests.Enqueue([
                new QStingOfTheScorpid(bot),
                new QVileFamiliars(bot),
                new QGalgarCactusAppleSurprise(bot)
            ]);
            Quests.Enqueue([
                new QSarkoth(bot)
            ]);
            Quests.Enqueue([
                new QSarkoth2(bot)
            ]);
            Quests.Enqueue([
                new QAPeonBurden(bot)
            ]);
            Quests.Enqueue([
                new QVanquishTheBetrayers(bot),
                new QCarryYourWeight(bot)
            ]);
            Quests.Enqueue([
                new QEncroachment(bot)
            ]);
            Quests.Enqueue([
                new QConscriptOfTheHorde(bot)
            ]);
            Quests.Enqueue([
                new QCrossroadsConscription(bot)
            ]);
            Quests.Enqueue([
                new QDisruptTheAttacks(bot)
            ]);
            Quests.Enqueue([
                new QTheDisruptionEnds(bot),
                new QSuppliesForTheCrossroads(bot),
            ]);
            Quests.Enqueue([
                new QPlainstriderMenace(bot),
                new QRaptorThieves(bot),
            ]);
            Quests.Enqueue([
                new QSouthseaFreebooters(bot),
            ]);
            Quests.Enqueue([
                new QTheZhevra(bot),
            ]);
            Quests.Enqueue([
                new QProwlersOfTheBarrens(bot),
            ]);
            Quests.Enqueue([
                new QHarpyRaiders(bot),
                new QCentaurBracers(bot),
            ]);
            Quests.Enqueue([
                new QKolkarLeaders(bot),
                new QHarpyLieutenants(bot),
            ]);
            Quests.Enqueue([
                new QSerenaBloodfeather(bot),
            ]);
            Quests.Enqueue([
                new QLetterToJinZil(bot),
            ]);
        }

        public Queue<List<IBotQuest>> Quests { get; }

        public override string ToString()
        {
            return $"[1-80] X5Horde1To80Profile (Shino)";
        }
    }
}