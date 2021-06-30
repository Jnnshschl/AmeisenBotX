using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Quest.Objects.Objectives;
using AmeisenBotX.Core.Quest.Objects.Quests;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Quest.Quests.Durotar.RazorHill
{
    internal class QEncroachment : BotQuest
    {
        public QEncroachment(AmeisenBotInterfaces bot)
            : base(bot, 837, "Encroachment", 6, 1,
                () => (bot.Objects.GetClosestWowUnitByNpcId(bot.Player.Position, new List<int> { 3139 }), new Vector3(274.99f, -4709.30f, 17.57f)),
                () => (bot.Objects.GetClosestWowUnitByNpcId(bot.Player.Position, new List<int> { 3139 }), new Vector3(274.99f, -4709.30f, 17.57f)),
                new List<IQuestObjective>()
                {
                    new QuestObjectiveChain(new List<IQuestObjective>()
                    {
                        new KillAndLootQuestObjective(bot, new List<int> { 3111 }, 4, 0, new List<List<Vector3>> {
                            new()
                            {
                                new Vector3(82.96f, -4643.34f, 37.46f),
                            },
                            new()
                            {
                                new Vector3(18.23f, -4617.23f, 44.73f),
                            },
                            new()
                            {
                                new Vector3(110.59f, -4572.87f, 58.22f),
                                new Vector3(128.80f, -4583.54f, 63.59f),
                                new Vector3(107.17f, -4552.57f, 56.81f),
                                new Vector3(85.42f, -4543.75f, 58.07f),
                                new Vector3(92.78f, -4557.56f, 54.70f),
                            },
                            new()
                            {
                                new Vector3(78.50f, -4623.69f, 44.11f),
                            },
                            new()
                            {
                                new Vector3(80.35f, -4406.12f, 43.48f),
                                new Vector3(58.28f, -4478.91f, 48.91f),
                                new Vector3(95.03f, -4476.37f, 40.75f),
                                new Vector3(111.28f, -4446.82f, 38.88f),
                            },
                        }),
                        new KillAndLootQuestObjective(bot, new List<int> { 3112 }, 4, 0, new List<List<Vector3>> {
                            new()
                            {
                                new Vector3(60.08f, -4298.42f, 64.12f),
                                new Vector3(58.33f, -4291.67f, 64.88f),
                                new Vector3(19.01f, -4251.52f, 72.92f),
                                new Vector3(22.13f, -4314.94f, 71.20f),
                                new Vector3(30.86f, -4335.03f, 71.35f),
                            },
                            new()
                            {
                                new Vector3(62.18f, -4454.37f, 46.83f),
                                new Vector3(51.03f, -4468.06f, 48.82f),
                                new Vector3(121.58f, -4477.19f, 37.94f),
                                new Vector3(117.88f, -4459.38f, 37.64f),
                            },
                            new()
                            {
                                new Vector3(77.80f, -4562.97f, 55.32f),
                                new Vector3(52.69f, -4613.99f, 46.73f),
                                new Vector3(124.53f, -4582.72f, 63.72f),
                            },
                        }),
                        new KillAndLootQuestObjective(bot, new List<int> { 3113 }, 4, 0, new List<List<Vector3>> {
                            new()
                            {
                                new Vector3(483.50f, -4148.08f, 24.82f),
                                new Vector3(449.46f, -4169.09f, 25.95f),
                                new Vector3(495.61f, -4170.69f, 25.78f),
                            },
                            new()
                            {
                                new Vector3(-57.78f, -3979.82f, 62.83f),
                                new Vector3(-86.05f, -4025.48f, 63.45f),
                                new Vector3(-68.29f, -4052.45f, 67.38f),
                                new Vector3(-57.18f, -4040.56f, 67.33f),
                                new Vector3(-52.60f, -4017.01f, 65.36f),
                            },
                            new()
                            {
                                new Vector3(424.51f, -4228.85f, 25.15f),
                                new Vector3(380.85f, -4282.82f, 25.98f),
                                new Vector3(406.92f, -4288.94f, 30.33f),
                                new Vector3(461.11f, -4298.42f, 25.21f),
                                new Vector3(484.98f, -4293.99f, 22.37f),
                                new Vector3(493.81f, -4256.06f, 21.15f),
                                new Vector3(439.98f, -4229.21f, 25.41f),
                            },
                        }),
                        new KillAndLootQuestObjective(bot, new List<int> { 3114 }, 4, 0, new List<List<Vector3>> {
                            new()
                            {
                                new Vector3(495.71f, -4196.06f, 24.16f),
                                new Vector3(461.62f, -4169.47f, 26.81f),
                                new Vector3(387.26f, -4175.69f, 27.16f),
                                new Vector3(342.62f, -4230.38f, 23.61f),
                                new Vector3(348.27f, -4282.86f, 23.74f),
                                new Vector3(410.89f, -4314.54f, 25.20f),
                                new Vector3(484.67f, -4348.81f, 27.17f),
                                new Vector3(492.11f, -4323.12f, 22.42f),
                            },
                            new()
                            {
                                new Vector3(-58.51f, -4029.94f, 66.55f),
                                new Vector3(-93.64f, -3984.71f, 61.06f),
                                new Vector3(-118.59f, -3985.38f, 58.97f),
                                new Vector3(-137.06f, -4010.05f, 59.34f),
                                new Vector3(-87.46f, -4045.82f, 64.50f),
                            },
                        }),
                    })
                })
        { }
    }
}