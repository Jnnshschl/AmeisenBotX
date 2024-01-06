using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Quest.Objects.Objectives;
using AmeisenBotX.Core.Engines.Quest.Objects.Quests;

namespace AmeisenBotX.Core.Engines.Quest.Quests.Grinder
{
    internal class QTheBarrensGrindToLevel16 : GrindingBotQuest
    {
        public QTheBarrensGrindToLevel16(AmeisenBotInterfaces bot)
            : base("TheBarrensGrindToLevel16",
                [
                    new QuestObjectiveChain(
                    [
                        new GrindingObjective(bot, 16, [
                            new()
                            {
                                new Vector3(-1731.59f, -3715.31f, 16.97f),
                                new Vector3(-1778.44f, -3724.77f, 10.15f),
                                new Vector3(-1779.11f, -3746.11f, 6.99f),
                                new Vector3(-1729.21f, -3839.87f, 10.49f),
                                new Vector3(-1710.46f, -3853.85f, 9.37f),
                                new Vector3(-1563.96f, -3906.81f, 12.58f),
                                new Vector3(-1364.40f, -3922.69f, 10.46f),
                                new Vector3(-1322.96f, -3884.70f, 11.28f),
                                new Vector3(-1316.59f, -3819.80f, 18.28f),
                                new Vector3(-1362.63f, -3753.26f, 59.90f),
                            },
                        ]),
                    ])
                ])
        { }
    }
}