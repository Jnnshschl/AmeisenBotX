using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Quest.Objects.Objectives;
using AmeisenBotX.Core.Engines.Quest.Objects.Quests;

namespace AmeisenBotX.Core.Engines.Quest.Quests.Grinder
{
    internal class QDesolaceGrindToLevel35 : GrindingBotQuest
    {
        public QDesolaceGrindToLevel35(AmeisenBotInterfaces bot)
            : base("DesolaceGrindToLevel35",
                [
                    new QuestObjectiveChain(
                    [
                        new GrindingObjective(bot, 35, [
                            new()
                            {
                                new Vector3(-711.86f, 1155.10f, 90.73f),
                                new Vector3(-1111.87f, 1151.47f, 92.02f),
                                new Vector3(-1110.17f, 925.52f, 89.26f),
                                new Vector3(-1078.40f, 881.74f, 91.84f),
                                new Vector3(-1010.09f, 874.88f, 92.61f),
                                new Vector3(-950.55f, 881.92f, 91.48f),
                                new Vector3(-930.34f, 888.47f, 91.91f),
                                new Vector3(-840.06f, 919.59f, 89.01f),
                                new Vector3(-753.98f, 955.86f, 91.10f),
                                new Vector3(-726.51f, 1083.33f, 90.35f),
                            },
                        ]),
                    ])
                ])
        { }
    }
}