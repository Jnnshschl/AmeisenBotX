using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Quest.Objects.Objectives;
using AmeisenBotX.Core.Quest.Objects.Quests;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Quest.Quests.TheBarrens.Crossroads
{
    internal class QTheDisruptionEnds : BotQuest
    {
        public QTheDisruptionEnds(WowInterface wowInterface)
            : base(wowInterface, 872, "The Disruption Ends", 9, 1,
                () => (wowInterface.Objects.GetClosestWowUnitByNpcId(wowInterface.Player.Position, new List<int> { 3429 }), new Vector3(-473.20f, -2595.70f, 103.81f)),
                () => (wowInterface.Objects.GetClosestWowUnitByNpcId(wowInterface.Player.Position, new List<int> { 3429 }), new Vector3(-473.20f, -2595.70f, 103.81f)),
                new List<IQuestObjective>()
                {
                    new QuestObjectiveChain(new List<IQuestObjective>()
                    {
                        new KillAndLootQuestObjective(wowInterface, new List<int> { 3269 }, 8, 0, new List<List<Vector3>> {
                            new()
                            {
                                new Vector3(-115.32f, -3200.88f, 93.61f),
                            },
                            new()
                            {
                                new Vector3(-18.10f, -3149.38f, 94.52f),
                                new Vector3(-81.98f, -3128.80f, 91.79f),
                                new Vector3(-102.80f, -3131.29f, 92.20f),
                                new Vector3(-106.77f, -3144.27f, 92.04f),
                                new Vector3(-49.21f, -3185.31f, 91.79f),
                            },
                            new()
                            {
                                new Vector3(-15.59f, -3326.32f, 95.39f),
                                new Vector3(-80.76f, -3370.37f, 93.26f),
                                new Vector3(-46.90f, -3400.80f, 91.75f),
                                new Vector3(-10.20f, -3402.27f, 88.09f),
                                new Vector3(-6.23f, -3368.26f, 91.74f),
                            },
                        }),
                        new KillAndLootQuestObjective(wowInterface, new List<int> { 3266 }, 8, 0, new List<List<Vector3>> {
                            new()
                            {
                                new Vector3(-55.60f, -3256.06f, 91.71f),
                            },
                            new()
                            {
                                new Vector3(-159.56f, -3325.61f, 92.92f),
                                new Vector3(-185.81f, -3284.44f, 91.79f),
                                new Vector3(-235.36f, -3343.92f, 91.79f),
                                new Vector3(-252.12f, -3381.21f, 96.79f),
                                new Vector3(-214.36f, -3387.89f, 91.79f),
                            },
                            new()
                            {
                                new Vector3(-91.00f, -3160.28f, 92.79f),
                                new Vector3(-112.09f, -3177.54f, 91.91f),
                                new Vector3(-85.02f, -3210.49f, 92.17f),
                                new Vector3(-80.74f, -3180.24f, 92.79f),
                            },
                            new()
                            {
                                new Vector3(-76.80f, -3345.80f, 92.17f),
                                new Vector3(-115.61f, -3382.43f, 92.30f),
                                new Vector3(-42.71f, -3418.10f, 91.87f),
                                new Vector3(-26.57f, -3412.59f, 91.67f),
                                new Vector3(-6.16f, -3381.21f, 90.04f),
                            },
                            new()
                            {
                                new Vector3(-47.43f, -3122.76f, 91.79f),
                            },
                        }),
                        new KillAndLootQuestObjective(wowInterface, new List<int> { 3438 }, 1, 5063, new List<List<Vector3>> {
                            new()
                            {
                                new Vector3(-214.25f, -3307.53f, 91.79f),
                            },
                        }),
                    })
                })
        { }
    }
}