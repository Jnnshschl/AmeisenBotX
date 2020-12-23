using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Core.Quest.Objects.Objectives;
using AmeisenBotX.Core.Quest.Objects.Quests;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Quest.Quests.Grinder
{
    class QTheBarrensGrindToLevel19 : GrindingBotQuest
    {
        public QTheBarrensGrindToLevel19(WowInterface wowInterface)
            : base("TheBarrensGrindToLevel19",
                new List<IQuestObjective>()
                {
                    new QuestObjectiveChain(new List<IQuestObjective>()
                    {
                        new GrindingObjective(wowInterface, 19, new List<List<Vector3>> {
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
                        }),
                    })
                })
        {}
    }
}
