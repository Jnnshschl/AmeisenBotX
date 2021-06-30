using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Quest.Objects.Objectives;
using AmeisenBotX.Core.Quest.Objects.Quests;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Quest.Quests.TheBarrens.OutpostStonetalon
{
    internal class QCentaurBracers : BotQuest
    {
        public QCentaurBracers(AmeisenBotInterfaces bot)
            : base(bot, 855, "Centaur Bracers", 9, 1,
                () => (bot.Objects.GetClosestWowUnitByNpcId(bot.Player.Position, new List<int> { 3389 }), new Vector3(-307.14f, -1971.95f, 96.48f)),
                () => (bot.Objects.GetClosestWowUnitByNpcId(bot.Player.Position, new List<int> { 3389 }), new Vector3(-307.14f, -1971.95f, 96.48f)),
                new List<IQuestObjective>()
                {
                    new QuestObjectiveChain(new List<IQuestObjective>()
                    {
                        new KillAndLootQuestObjective(bot, new List<int> { 3272,3273,3274,3275,3397,5837,5838,5841,9523,9524,3394,3395,3396,9456 }, 15, 5030, new List<List<Vector3>> {
                            new()
                            {
                                new Vector3(-1478.94f, -3013.48f, 91.79f),
                                new Vector3(-1550.39f, -2880.64f, 91.79f),
                                new Vector3(-1533.78f, -2976.79f, 91.90f),
                                new Vector3(-1493.20f, -3043.02f, 91.79f),
                                new Vector3(-1482.03f, -3032.52f, 91.79f),
                            },
                            new()
                            {
                                new Vector3(-1417.40f, -2738.95f, 91.79f),
                                new Vector3(-1401.19f, -2756.57f, 91.79f),
                                new Vector3(-1394.55f, -2734.76f, 91.79f),
                                new Vector3(-1422.80f, -2682.44f, 93.56f),
                            },
                            new()
                            {
                                new Vector3(-895.98f, -2926.44f, 91.79f),
                            },
                            new()
                            {
                                new Vector3(-912.21f, -2217.01f, 93.63f),
                                new Vector3(-850.23f, -2116.41f, 92.33f),
                                new Vector3(-876.65f, -1976.44f, 93.38f),
                                new Vector3(-937.95f, -1867.95f, 102.10f),
                                new Vector3(-985.42f, -1849.07f, 94.05f),
                                new Vector3(-1071.06f, -1910.30f, 91.81f),
                                new Vector3(-1186.64f, -2000.88f, 91.79f),
                                new Vector3(-1237.82f, -2084.81f, 91.55f),
                                new Vector3(-1219.00f, -2176.22f, 91.71f),
                                new Vector3(-1148.40f, -2313.98f, 94.49f),
                                new Vector3(-1010.05f, -2330.36f, 92.17f),
                            },
                            new()
                            {
                                new Vector3(-1255.58f, -2294.36f, 94.10f),
                            },
                            new()
                            {
                                new Vector3(-861.46f, -2746.88f, 91.79f),
                                new Vector3(-863.25f, -2728.11f, 91.79f),
                                new Vector3(-877.40f, -2753.24f, 92.04f),
                                new Vector3(-892.43f, -2814.67f, 94.71f),
                                new Vector3(-876.74f, -2789.97f, 92.67f),
                            },
                            new()
                            {
                                new Vector3(-1016.74f, -2719.69f, 94.76f),
                            },
                            new()
                            {
                                new Vector3(-1351.32f, -2986.78f, 92.92f),
                                new Vector3(-1352.25f, -3052.89f, 92.53f),
                                new Vector3(-1342.26f, -3110.99f, 91.79f),
                                new Vector3(-1334.89f, -3135.21f, 91.79f),
                                new Vector3(-1283.28f, -3152.77f, 98.52f),
                                new Vector3(-1160.35f, -3168.60f, 91.79f),
                                new Vector3(-1129.16f, -3147.13f, 94.13f),
                                new Vector3(-1092.69f, -2805.23f, 92.29f),
                                new Vector3(-1106.19f, -2789.72f, 91.79f),
                                new Vector3(-1210.58f, -2725.92f, 106.76f),
                                new Vector3(-1246.53f, -2746.67f, 91.79f),
                                new Vector3(-1287.89f, -2820.94f, 93.16f),
                            },
                            new()
                            {
                                new Vector3(-61.22f, -1630.93f, 91.79f),
                                new Vector3(-181.21f, -1641.30f, 92.54f),
                                new Vector3(-202.10f, -1648.94f, 91.79f),
                                new Vector3(-235.02f, -1662.62f, 91.79f),
                                new Vector3(-143.53f, -2007.58f, 91.79f),
                                new Vector3(-62.29f, -2243.04f, 92.50f),
                                new Vector3(-42.45f, -2272.18f, 93.09f),
                                new Vector3(261.55f, -1950.92f, 91.92f),
                                new Vector3(286.11f, -1913.00f, 91.79f),
                                new Vector3(192.64f, -1772.60f, 91.79f),
                                new Vector3(-53.90f, -1633.82f, 91.67f),
                            },
                            new()
                            {
                                new Vector3(-911.83f, -2942.02f, 91.79f),
                            },
                        }),
                    })
                })
        { }
    }
}