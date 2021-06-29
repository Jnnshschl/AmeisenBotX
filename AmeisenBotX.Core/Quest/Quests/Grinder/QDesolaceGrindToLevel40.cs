using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Quest.Objects.Objectives;
using AmeisenBotX.Core.Quest.Objects.Quests;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Quest.Quests.Grinder
{
    internal class QDesolaceGrindToLevel40 : GrindingBotQuest
    {
        public QDesolaceGrindToLevel40(WowInterface wowInterface)
            : base("DesolaceGrindToLevel40",
                new List<IQuestObjective>()
                {
                    new QuestObjectiveChain(new List<IQuestObjective>()
                    {
                        new GrindingObjective(wowInterface, 40, new List<List<Vector3>> {
                            new()
                            {
                                new Vector3(-1884.74f, 1314.40f, 87.69f),
                                new Vector3(-1962.75f, 1267.75f, 91.68f),
                                new Vector3(-2048.95f, 1214.00f, 107.50f),
                                new Vector3(-2041.08f, 1152.25f, 116.73f),
                                new Vector3(-1822.87f, 818.86f, 103.17f),
                                new Vector3(-1747.88f, 806.49f, 101.52f),
                                new Vector3(-1683.64f, 817.03f, 97.07f),
                                new Vector3(-1590.06f, 855.55f, 114.45f),
                                new Vector3(-1517.33f, 915.28f, 90.07f),
                                new Vector3(-1618.47f, 1242.85f, 90.80f),
                                new Vector3(-1655.57f, 1284.73f, 90.63f),
                            },
                        }),
                    })
                })
        { }
    }
}