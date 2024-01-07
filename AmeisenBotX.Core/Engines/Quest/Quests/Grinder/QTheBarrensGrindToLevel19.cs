using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Quest.Objects.Objectives;
using AmeisenBotX.Core.Engines.Quest.Objects.Quests;

namespace AmeisenBotX.Core.Engines.Quest.Quests.Grinder
{
    internal class QTheBarrensGrindToLevel19(AmeisenBotInterfaces bot) : GrindingBotQuest("TheBarrensGrindToLevel19",
            [
                    new QuestObjectiveChain(
                    [
                        new GrindingObjective(bot, 19, [
                            new()
                            {
                                new Vector3(-1905.31f, -3481.39f, 41.36f),
                                new Vector3(-2047.22f, -3469.00f, 99.92f),
                                new Vector3(-2134.64f, -3506.05f, 92.45f),
                                new Vector3(-2228.27f, -3712.82f, 91.94f),
                                new Vector3(-2230.87f, -3729.80f, 92.07f),
                                new Vector3(-2220.71f, -3770.01f, 95.78f),
                                new Vector3(-2214.54f, -3773.77f, 95.77f),
                                new Vector3(-1938.34f, -3713.84f, 7.20f),
                                new Vector3(-1877.66f, -3660.91f, 10.44f),
                            },
                        ]),
                    ])
                ])
    {
    }
}