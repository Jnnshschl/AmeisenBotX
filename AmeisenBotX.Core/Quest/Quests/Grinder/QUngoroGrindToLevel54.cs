using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Core.Quest.Objects.Objectives;
using AmeisenBotX.Core.Quest.Objects.Quests;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Quest.Quests.Grinder
{
    internal class QUngoroGrindToLevel54 : GrindingBotQuest
    {
        public QUngoroGrindToLevel54(WowInterface wowInterface)
            : base("UngoroGrindToLevel54",
                new List<IQuestObjective>()
                {
                    new QuestObjectiveChain(new List<IQuestObjective>()
                    {
                        new GrindingObjective(wowInterface, 54, new List<List<Vector3>> {
                            new()
                            {
                                new Vector3(-7869.45f, -1589.07f, -262.72f),
                                new Vector3(-7939.71f, -1625.50f, -273.19f),
                                new Vector3(-7979.99f, -1679.12f, -269.71f),
                                new Vector3(-8019.15f, -1756.37f, -271.33f),
                                new Vector3(-7908.42f, -1979.66f, -271.92f),
                                new Vector3(-7483.21f, -2114.33f, -272.36f),
                                new Vector3(-7215.42f, -2153.10f, -271.38f),
                                new Vector3(-7205.24f, -2152.41f, -270.62f),
                                new Vector3(-7085.31f, -2079.43f, -268.27f),
                                new Vector3(-7080.86f, -1981.56f, -270.47f),
                                new Vector3(-7121.37f, -1888.57f, -272.10f),
                                new Vector3(-7146.04f, -1836.71f, -271.63f),
                                new Vector3(-7212.90f, -1776.50f, -276.99f),
                            },
                        }),
                    })
                })
        { }
    }
}