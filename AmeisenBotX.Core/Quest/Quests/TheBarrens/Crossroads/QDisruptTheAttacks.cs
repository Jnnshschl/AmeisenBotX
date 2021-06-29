using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Quest.Objects.Objectives;
using AmeisenBotX.Core.Quest.Objects.Quests;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Quest.Quests.TheBarrens.Crossroads
{
    internal class QDisruptTheAttacks : BotQuest
    {
        public QDisruptTheAttacks(WowInterface wowInterface)
            : base(wowInterface, 871, "Disrupt the Attacks", 9, 1,
                () => (wowInterface.Objects.GetClosestWowUnitByNpcId(wowInterface.Player.Position, new List<int> { 3429 }), new Vector3(-473.20f, -2595.70f, 103.81f)),
                () => (wowInterface.Objects.GetClosestWowUnitByNpcId(wowInterface.Player.Position, new List<int> { 3429 }), new Vector3(-473.20f, -2595.70f, 103.81f)),
                new List<IQuestObjective>()
                {
                    new QuestObjectiveChain(new List<IQuestObjective>()
                    {
                        new KillAndLootQuestObjective(wowInterface, new List<int> { 3267 }, 8, 0, new List<List<Vector3>> {
                            new()
                            {
                                new Vector3(-120.77f, -2820.86f, 91.79f),
                                new Vector3(-148.08f, -2846.17f, 95.06f),
                                new Vector3(-137.61f, -2887.90f, 92.92f),
                                new Vector3(-77.10f, -2886.17f, 92.03f),
                                new Vector3(-89.19f, -2852.54f, 91.79f),
                            },
                            new()
                            {
                                new Vector3(-126.96f, -3005.08f, 91.79f),
                            },
                            new()
                            {
                                new Vector3(-43.21f, -2813.28f, 92.99f),
                            },
                            new()
                            {
                                new Vector3(-80.01f, -2752.45f, 91.79f),
                            },
                            new()
                            {
                                new Vector3(-187.77f, -2992.73f, 91.92f),
                                new Vector3(-193.82f, -2943.92f, 91.79f),
                                new Vector3(-209.37f, -2953.74f, 91.79f),
                                new Vector3(-229.86f, -3018.29f, 91.79f),
                                new Vector3(-224.10f, -3036.88f, 91.79f),
                                new Vector3(-189.59f, -3033.26f, 91.79f),
                                new Vector3(-188.73f, -3027.56f, 91.79f),
                            },
                        }),
                        new KillAndLootQuestObjective(wowInterface, new List<int> { 3268 }, 8, 0, new List<List<Vector3>> {
                            new()
                            {
                                new Vector3(-183.96f, -2961.07f, 91.92f),
                                new Vector3(-207.64f, -2993.85f, 91.79f),
                                new Vector3(-207.32f, -3030.23f, 91.79f),
                                new Vector3(-165.21f, -3037.72f, 91.79f),
                                new Vector3(-155.78f, -3011.95f, 91.79f),
                            },
                            new()
                            {
                                new Vector3(-218.70f, -2922.10f, 91.79f),
                            },
                            new()
                            {
                                new Vector3(-111.91f, -2855.58f, 92.42f),
                                new Vector3(-116.86f, -2924.42f, 91.85f),
                                new Vector3(-99.20f, -2952.35f, 92.00f),
                                new Vector3(-48.97f, -2893.44f, 91.89f),
                                new Vector3(-82.61f, -2864.88f, 91.79f),
                            },
                        }),
                        new KillAndLootQuestObjective(wowInterface, new List<int> { 3265 }, 3, 0, new List<List<Vector3>> {
                            new()
                            {
                                new Vector3(1.45f, -3399.94f, 85.36f),
                            },
                            new()
                            {
                                new Vector3(-175.26f, -3008.49f, 91.83f),
                            },
                            new()
                            {
                                new Vector3(-92.78f, -2941.98f, 93.04f),
                            },
                            new()
                            {
                                new Vector3(-174.85f, -3375.98f, 93.17f),
                            },
                            new()
                            {
                                new Vector3(16.35f, -3237.44f, 94.29f),
                                new Vector3(-38.80f, -3234.81f, 91.89f),
                                new Vector3(-23.09f, -3266.17f, 92.18f),
                            },
                            new()
                            {
                                new Vector3(-116.46f, -2960.58f, 91.79f),
                            },
                            new()
                            {
                                new Vector3(-96.41f, -2824.85f, 91.99f),
                            },
                        }),
                    })
                })
        { }
    }
}