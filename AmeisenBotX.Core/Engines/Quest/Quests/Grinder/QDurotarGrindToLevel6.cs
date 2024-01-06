using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Quest.Objects.Objectives;
using AmeisenBotX.Core.Engines.Quest.Objects.Quests;

namespace AmeisenBotX.Core.Engines.Quest.Quests.Grinder
{
    internal class QDurotarGrindToLevel6 : GrindingBotQuest
    {
        public QDurotarGrindToLevel6(AmeisenBotInterfaces bot)
            : base("DurotarGrindToLevel6",
                [
                    new QuestObjectiveChain(
                    [
                        new GrindingObjective(bot, 6,
                        [
                            new()
                            {
                                new Vector3(-787.39f, -4246.54f, 52.66f),
                                new Vector3(-788.54f, -4286.46f, 52.82f),
                                new Vector3(-756.20f, -4352.79f, 52.21f),
                                new Vector3(-612.62f, -4448.09f, 45.59f),
                                new Vector3(-619.22f, -4382.64f, 43.22f),
                                new Vector3(-676.94f, -4114.47f, 39.30f),
                                new Vector3(-717.00f, -4150.75f, 30.07f),
                                new Vector3(-747.26f, -4181.42f, 30.24f),
                            },
                            new()
                            {
                                new Vector3(-346.63f, -4045.22f, 51.11f),
                                new Vector3(-610.69f, -4084.09f, 77.46f),
                                new Vector3(-607.08f, -4113.04f, 74.95f),
                                new Vector3(-549.24f, -4421.25f, 42.10f),
                                new Vector3(-448.05f, -4486.25f, 54.76f),
                                new Vector3(-210.93f, -4448.35f, 68.28f),
                                new Vector3(-47.92f, -4312.65f, 68.79f),
                                new Vector3(-43.84f, -4274.30f, 68.29f),
                                new Vector3(-43.41f, -4226.15f, 63.76f),
                            },
                        ],
                        [
                            new(-565.428f, -4214.2f, 41.59f)
                        ]),
                    ])
                ])
        { }
    }
}