using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Core.Quest.Objects.Objectives;
using AmeisenBotX.Core.Quest.Objects.Quests;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Quest.Quests.Grinder
{
    internal class QStonetalonGrindToLevel31 : GrindingBotQuest
    {
        public QStonetalonGrindToLevel31(WowInterface wowInterface)
            : base("StonetalonGrindToLevel31",
                new List<IQuestObjective>()
                {
                    new QuestObjectiveChain(new List<IQuestObjective>()
                    {
                        new GrindingObjective(wowInterface, 31, new List<List<Vector3>> {
                            new()
                            {
                                new Vector3(652.70f, 1937.45f, -5.25f),
                                new Vector3(600.10f, 1819.81f, -9.06f),
                                new Vector3(672.81f, 1741.93f, -19.42f),
                                new Vector3(790.73f, 1742.48f, -20.34f),
                            },
                            new()
                            {
                                new Vector3(948.89f, 1679.82f, -12.18f),
                                new Vector3(1010.01f, 1726.79f, -9.78f),
                                new Vector3(877.56f, 1798.55f, -6.31f),
                            },
                            new()
                            {
                                new Vector3(701.55f, 1551.92f, -22.45f),
                                new Vector3(612.17f, 1550.70f, -12.91f),
                                new Vector3(644.38f, 1439.96f, -5.07f),
                                new Vector3(776.02f, 1425.23f, -14.05f),
                                new Vector3(749.44f, 1514.12f, -21.27f),
                            },
                        }),
                    })
                })
        { }
    }
}