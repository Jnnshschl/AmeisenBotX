using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Quest.Objects.Objectives;
using AmeisenBotX.Core.Quest.Objects.Quests;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Quest.Quests.TheBarrens.Crossroads
{
    internal class QProwlersOfTheBarrens : BotQuest
    {
        public QProwlersOfTheBarrens(WowInterface wowInterface)
            : base(wowInterface, 903, "Prowlers of the Barrens", 10, 1,
                () => (wowInterface.Objects.GetClosestWowUnitByNpcId(wowInterface.Player.Position, new List<int> { 3338 }), new Vector3(-482.48f, -2670.19f, 97.52f)),
                () => (wowInterface.Objects.GetClosestWowUnitByNpcId(wowInterface.Player.Position, new List<int> { 3338 }), new Vector3(-482.48f, -2670.19f, 97.52f)),
                new List<IQuestObjective>()
                {
                    new QuestObjectiveChain(new List<IQuestObjective>()
                    {
                        new KillAndLootQuestObjective(wowInterface, new List<int> { 3425 }, 7, 5096, new List<List<Vector3>> {
                            new()
                            {
                                new Vector3(1081.71f, -3481.79f, 78.82f),
                            },
                            new()
                            {
                                new Vector3(564.40f, -2764.35f, 91.79f),
                                new Vector3(526.19f, -2777.86f, 91.79f),
                                new Vector3(495.84f, -3011.28f, 92.29f),
                                new Vector3(522.09f, -3034.60f, 91.79f),
                                new Vector3(614.77f, -3031.96f, 91.79f),
                                new Vector3(627.21f, -3016.09f, 91.79f),
                                new Vector3(701.62f, -2848.94f, 93.54f),
                                new Vector3(697.51f, -2823.20f, 91.79f),
                            },
                            new()
                            {
                                new Vector3(-305.94f, -1561.00f, 92.15f),
                                new Vector3(-326.44f, -1572.02f, 92.45f),
                                new Vector3(-328.42f, -1588.06f, 92.08f),
                                new Vector3(-212.86f, -1551.53f, 92.42f),
                                new Vector3(-163.34f, -1516.19f, 91.79f),
                                new Vector3(-223.01f, -1528.96f, 91.79f),
                            },
                            new()
                            {
                                new Vector3(250.10f, -1471.37f, 92.04f),
                                new Vector3(227.48f, -1460.16f, 91.79f),
                                new Vector3(213.95f, -1461.04f, 91.79f),
                                new Vector3(92.16f, -1491.53f, 91.79f),
                                new Vector3(-30.43f, -1562.82f, 91.98f),
                                new Vector3(-21.47f, -1577.63f, 91.79f),
                                new Vector3(24.13f, -1632.79f, 91.79f),
                                new Vector3(207.82f, -1544.65f, 91.79f),
                            },
                            new()
                            {
                                new Vector3(-1042.86f, -3227.71f, 91.79f),
                            },
                            new()
                            {
                                new Vector3(1187.22f, -3830.57f, 27.85f),
                                new Vector3(1251.83f, -3814.86f, 30.26f),
                                new Vector3(1120.09f, -3787.30f, 31.09f),
                            },
                            new()
                            {
                                new Vector3(-495.23f, -1859.01f, 91.79f),
                                new Vector3(-578.37f, -1658.35f, 91.79f),
                                new Vector3(-726.70f, -1678.04f, 91.79f),
                                new Vector3(-760.05f, -1684.79f, 91.79f),
                                new Vector3(-785.75f, -1791.60f, 91.79f),
                                new Vector3(-779.10f, -1812.64f, 91.79f),
                                new Vector3(-539.66f, -1890.55f, 93.04f),
                            },
                            new()
                            {
                                new Vector3(-712.65f, -3424.57f, 91.79f),
                            },
                            new()
                            {
                                new Vector3(-336.33f, -1692.76f, 92.17f),
                            },
                            new()
                            {
                                new Vector3(-851.94f, -3356.92f, 91.79f),
                            },
                            new()
                            {
                                new Vector3(-1150.96f, -3411.59f, 91.79f),
                                new Vector3(-1144.96f, -3390.34f, 91.71f),
                                new Vector3(-1142.66f, -3314.15f, 91.92f),
                                new Vector3(-1157.84f, -3378.62f, 91.71f),
                            },
                            new()
                            {
                                new Vector3(-1045.08f, -1781.89f, 91.71f),
                                new Vector3(-1050.82f, -1792.73f, 91.71f),
                                new Vector3(-927.17f, -1819.05f, 92.42f),
                                new Vector3(-951.07f, -1808.73f, 91.79f),
                            },
                            new()
                            {
                                new Vector3(-382.08f, -3782.55f, 29.07f),
                                new Vector3(-283.73f, -3777.42f, 31.17f),
                                new Vector3(-245.77f, -3773.56f, 26.74f),
                                new Vector3(-243.73f, -3722.56f, 29.31f),
                                new Vector3(-328.57f, -3742.22f, 27.83f),
                            },
                            new()
                            {
                                new Vector3(1079.38f, -3519.58f, 71.26f),
                            },
                        }),
                    })
                })
        { }
    }
}