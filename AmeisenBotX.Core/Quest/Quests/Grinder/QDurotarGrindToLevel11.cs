using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Quest.Objects.Objectives;
using AmeisenBotX.Core.Quest.Objects.Quests;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Quest.Quests.Grinder
{
    internal class QDurotarGrindToLevel11 : GrindingBotQuest
    {
        public QDurotarGrindToLevel11(WowInterface wowInterface)
            : base("DurotarGrindToLevel11",
                new List<IQuestObjective>()
                {
                    new QuestObjectiveChain(new List<IQuestObjective>()
                    {
                        new GrindingObjective(wowInterface, 11, new List<List<Vector3>> {
                            new()
                            {
                                new Vector3(483.50f, -4148.08f, 24.82f),
                                new Vector3(387.26f, -4175.69f, 27.16f),
                                new Vector3(342.62f, -4230.38f, 23.61f),
                                new Vector3(348.27f, -4282.86f, 23.74f),
                                new Vector3(410.89f, -4314.54f, 25.20f),
                                new Vector3(484.67f, -4348.81f, 27.17f),
                                new Vector3(492.11f, -4323.12f, 22.42f),
                                new Vector3(495.71f, -4196.06f, 24.16f),
                                new Vector3(495.61f, -4170.69f, 25.78f),
                            },
                            new()
                            {
                                new Vector3(-52.60f, -4017.01f, 65.36f),
                                new Vector3(-57.78f, -3979.82f, 62.83f),
                                new Vector3(-118.59f, -3985.38f, 58.97f),
                                new Vector3(-137.06f, -4010.05f, 59.34f),
                                new Vector3(-87.46f, -4045.82f, 64.50f),
                                new Vector3(-68.29f, -4052.45f, 67.38f),
                                new Vector3(-57.18f, -4040.56f, 67.33f),
                            },
                        }),
                    })
                })
        { }
    }
}