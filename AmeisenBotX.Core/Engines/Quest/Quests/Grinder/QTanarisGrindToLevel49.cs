using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Quest.Objects.Objectives;
using AmeisenBotX.Core.Engines.Quest.Objects.Quests;

namespace AmeisenBotX.Core.Engines.Quest.Quests.Grinder
{
    internal class QTanarisGrindToLevel49(AmeisenBotInterfaces bot) : GrindingBotQuest("TanarisGrindToLevel49",
            [
                    new QuestObjectiveChain(
                    [
                        new GrindingObjective(bot, 49, [
                            new()
                            {
                                new Vector3(-7828.86f, -5111.13f, 4.30f),
                                new Vector3(-7854.17f, -5086.78f, 5.58f),
                                new Vector3(-8052.26f, -5110.95f, 14.24f),
                                new Vector3(-8085.70f, -5155.19f, 10.54f),
                                new Vector3(-8110.47f, -5213.92f, 7.62f),
                                new Vector3(-8096.42f, -5381.65f, 7.03f),
                                new Vector3(-7981.71f, -5492.10f, 7.53f),
                                new Vector3(-7970.46f, -5489.10f, 7.53f),
                                new Vector3(-7959.12f, -5481.83f, 0.29f),
                                new Vector3(-7945.06f, -5469.77f, 7.53f),
                            },
                        ]),
                    ])
                ])
    {
    }
}