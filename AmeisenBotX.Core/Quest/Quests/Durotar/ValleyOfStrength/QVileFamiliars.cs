using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Quest.Objects.Objectives;
using AmeisenBotX.Core.Quest.Objects.Quests;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Quest.Quests.Durotar.ValleyOfStrength
{
    internal class QVileFamiliars : BotQuest
    {
        public QVileFamiliars(AmeisenBotInterfaces bot)
            : base(bot, 792, "Vile Familiars", 2, 1,
                () => (bot.Objects.GetClosestWowUnitByNpcId(bot.Player.Position, new List<int> { 3145 }), new Vector3(-629.05f, -4228.06f, 38.23f)),
                () => (bot.Objects.GetClosestWowUnitByNpcId(bot.Player.Position, new List<int> { 3145 }), new Vector3(-629.05f, -4228.06f, 38.23f)),
                new List<IQuestObjective>()
                {
                    new QuestObjectiveChain(new List<IQuestObjective>()
                    {
                        new KillAndLootQuestObjective(bot, new List<int> { 3101 }, 8, 0, new List<List<Vector3>> {
                            new()
                            {
                                new Vector3(-117.28f, -4217.52f, 54.48f),
                                new Vector3(-246.56f, -4279.26f, 61.64f),
                                new Vector3(-253.59f, -4316.13f, 56.04f),
                                new Vector3(-252.91f, -4381.57f, 62.57f),
                                new Vector3(-210.93f, -4448.35f, 68.28f),
                                new Vector3(-47.92f, -4312.65f, 68.79f),
                                new Vector3(-43.84f, -4274.30f, 68.29f),
                                new Vector3(-43.41f, -4226.15f, 63.76f),
                                new Vector3(-49.32f, -4222.96f, 62.27f),
                            },
                        }),
                    })
                })
        { }
    }
}