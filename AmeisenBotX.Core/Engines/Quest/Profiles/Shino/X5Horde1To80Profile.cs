using AmeisenBotX.Core.Engines.Quest.Objects.Quests;
using AmeisenBotX.Core.Engines.Quest.Quests.Durotar.RazorHill;
using AmeisenBotX.Core.Engines.Quest.Quests.Durotar.ValleyOfStrength;
using AmeisenBotX.Core.Engines.Quest.Quests.TheBarrens.Crossroads;
using AmeisenBotX.Core.Engines.Quest.Quests.TheBarrens.OutpostBridge;
using AmeisenBotX.Core.Engines.Quest.Quests.TheBarrens.OutpostStonetalon;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Quest.Profiles.Shino
{
    internal class X5Horde1To80Profile : QuestProfile
    {
        public X5Horde1To80Profile(AmeisenBotInterfaces bot) : base(bot)
        {
            Quests = new Queue<List<IBotQuest>>();
            Quests.Enqueue(new List<IBotQuest>() {
                new QYourPlaceInTheWorld(bot)
            });
            Quests.Enqueue(new List<IBotQuest>() {
                new QCuttingTeeth(bot)
            });
            Quests.Enqueue(new List<IBotQuest>() {
                new QStingOfTheScorpid(bot),
                new QVileFamiliars(bot),
                new QGalgarCactusAppleSurprise(bot)
            });
            Quests.Enqueue(new List<IBotQuest>() {
                new QSarkoth(bot)
            });
            Quests.Enqueue(new List<IBotQuest>() {
                new QSarkoth2(bot)
            });
            Quests.Enqueue(new List<IBotQuest>() {
                new QAPeonBurden(bot)
            });
            Quests.Enqueue(new List<IBotQuest>() {
                new QVanquishTheBetrayers(bot),
                new QCarryYourWeight(bot)
            });
            Quests.Enqueue(new List<IBotQuest>() {
                new QEncroachment(bot)
            });
            Quests.Enqueue(new List<IBotQuest>() {
                new QConscriptOfTheHorde(bot)
            });
            Quests.Enqueue(new List<IBotQuest>() {
                new QCrossroadsConscription(bot)
            });
            Quests.Enqueue(new List<IBotQuest>() {
                new QDisruptTheAttacks(bot)
            });
            Quests.Enqueue(new List<IBotQuest>() {
                new QTheDisruptionEnds(bot),
                new QSuppliesForTheCrossroads(bot),
            });
            Quests.Enqueue(new List<IBotQuest>() {
                new QPlainstriderMenace(bot),
                new QRaptorThieves(bot),
            });
            Quests.Enqueue(new List<IBotQuest>() {
                new QSouthseaFreebooters(bot),
            });
            Quests.Enqueue(new List<IBotQuest>() {
                new QTheZhevra(bot),
            });
            Quests.Enqueue(new List<IBotQuest>() {
                new QProwlersOfTheBarrens(bot),
            });
            Quests.Enqueue(new List<IBotQuest>() {
                new QHarpyRaiders(bot),
                new QCentaurBracers(bot),
            });
            Quests.Enqueue(new List<IBotQuest>() {
                new QKolkarLeaders(bot),
                new QHarpyLieutenants(bot),
            });
            Quests.Enqueue(new List<IBotQuest>() {
                new QSerenaBloodfeather(bot),
            });
            Quests.Enqueue(new List<IBotQuest>() {
                new QLetterToJinZil(bot),
            });
        }

        public Queue<List<IBotQuest>> Quests { get; }

        public override string ToString()
        {
            return $"[1-80] X5Horde1To80Profile (Shino)";
        }
    }
}