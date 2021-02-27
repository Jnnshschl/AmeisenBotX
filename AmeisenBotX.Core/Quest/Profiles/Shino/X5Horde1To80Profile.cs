using AmeisenBotX.Core.Quest.Objects.Quests;
using AmeisenBotX.Core.Quest.Quests.Durotar.RazorHill;
using AmeisenBotX.Core.Quest.Quests.Durotar.ValleyOfStrength;
using AmeisenBotX.Core.Quest.Quests.TheBarrens.Crossroads;
using AmeisenBotX.Core.Quest.Quests.TheBarrens.OutpostBridge;
using AmeisenBotX.Core.Quest.Quests.TheBarrens.OutpostStonetalon;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Quest.Profiles.Shino
{
    internal class X5Horde1To80Profile : IQuestProfile
    {
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
                new QCarryYourWeight(wowInterface)
            });
            Quests.Enqueue(new List<IBotQuest>() {
                new QEncroachment(wowInterface)
            });
            Quests.Enqueue(new List<IBotQuest>() {
                new QConscriptOfTheHorde(wowInterface)
            });
            Quests.Enqueue(new List<IBotQuest>() {
                new QCrossroadsConscription(wowInterface)
            });
            Quests.Enqueue(new List<IBotQuest>() {
                new QDisruptTheAttacks(wowInterface)
            });
            Quests.Enqueue(new List<IBotQuest>() {
                new QTheDisruptionEnds(wowInterface),
                new QSuppliesForTheCrossroads(wowInterface),
            });
            Quests.Enqueue(new List<IBotQuest>() {
                new QPlainstriderMenace(wowInterface),
                new QRaptorThieves(wowInterface),
            });
            Quests.Enqueue(new List<IBotQuest>() {
                new QSouthseaFreebooters(wowInterface),
            });
            Quests.Enqueue(new List<IBotQuest>() {
                new QTheZhevra(wowInterface),
            });
            Quests.Enqueue(new List<IBotQuest>() {
                new QProwlersOfTheBarrens(wowInterface),
            });
            Quests.Enqueue(new List<IBotQuest>() {
                new QHarpyRaiders(wowInterface),
                new QCentaurBracers(wowInterface),
            });
            Quests.Enqueue(new List<IBotQuest>() {
                new QKolkarLeaders(wowInterface),
                new QHarpyLieutenants(wowInterface),
            });
            Quests.Enqueue(new List<IBotQuest>() {
                new QSerenaBloodfeather(wowInterface),
            });
            Quests.Enqueue(new List<IBotQuest>() {
                new QLetterToJinZil(wowInterface),
            });
        }

        public Queue<List<IBotQuest>> Quests { get; }

        public override string ToString()
        {
            return $"[1-80] X5Horde1To80Profile (Shino)";
        }
    }
}