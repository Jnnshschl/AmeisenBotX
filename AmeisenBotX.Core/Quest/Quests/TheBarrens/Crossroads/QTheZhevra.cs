using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Core.Quest.Objects.Objectives;
using AmeisenBotX.Core.Quest.Objects.Quests;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Quest.Quests.TheBarrens.Crossroads
{
    class QTheZhevra : BotQuest
    {
        public QTheZhevra(WowInterface wowInterface)
            : base(wowInterface, 845, "The Zhevra", 10, 1,
                () => (wowInterface.ObjectManager.GetClosestWowUnitByNpcId(new List<int> { 3338 }), new Vector3(-482.48f, -2670.19f, 97.52f)),
                () => (wowInterface.ObjectManager.GetClosestWowUnitByNpcId(new List<int> { 3338 }), new Vector3(-482.48f, -2670.19f, 97.52f)),
                new List<IQuestObjective>()
                {
                    new QuestObjectiveChain(new List<IQuestObjective>()
                    {
                        new KillAndLootQuestObjective(wowInterface, new List<int> { 3242,3426,3466,5831 }, 4, 5086, new List<List<Vector3>> {
                            new()
                            {
                                new Vector3(-2920.79f, -1940.51f, 92.13f),
                                new Vector3(-3148.13f, -2116.10f, 91.79f),
                                new Vector3(-1774.79f, -3592.18f, 92.24f),
                                new Vector3(-706.49f, -3960.36f, 24.73f),
                                new Vector3(360.36f, -3690.30f, 28.56f),
                                new Vector3(857.22f, -3491.63f, 94.36f),
                                new Vector3(1161.22f, -3279.11f, 91.77f),
                                new Vector3(702.26f, -1822.08f, 91.79f),
                                new Vector3(622.80f, -1673.04f, 91.79f),
                                new Vector3(-282.78f, -1055.41f, 40.17f),
                                new Vector3(-2486.04f, -1782.38f, 93.29f),
                            },
                        }),
                    })
                })
        {}
    }
}
