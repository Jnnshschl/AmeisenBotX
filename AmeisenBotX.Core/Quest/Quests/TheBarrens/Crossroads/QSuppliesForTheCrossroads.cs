using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Core.Quest.Objects.Objectives;
using AmeisenBotX.Core.Quest.Objects.Quests;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Quest.Quests.TheBarrens.Crossroads
{
    class QSuppliesForTheCrossroads : BotQuest
    {
        public QSuppliesForTheCrossroads(WowInterface wowInterface)
            : base(wowInterface, 5041, "Supplies for the Crossroads", 9, 1,
                () => (wowInterface.ObjectManager.GetClosestWowUnitByNpcId(new List<int> { 3429 }), new Vector3(-473.20f, -2595.70f, 103.81f)),
                () => (wowInterface.ObjectManager.GetClosestWowUnitByNpcId(new List<int> { 3429 }), new Vector3(-473.20f, -2595.70f, 103.81f)),
                new List<IQuestObjective>()
                {
                    new QuestObjectiveChain(new List<IQuestObjective>()
                    {
                        new CollectQuestObjective(wowInterface, 12708, 1, new List<int> { 175708 }, new List<Vector3> {
                            new Vector3(-212.20f, -3292.28f, 91.67f),
                            new Vector3(-60.17f, -3398.86f, 91.72f),
                            new Vector3(-230.87f, -3307.60f, 91.67f),
                            new Vector3(-69.64f, -3390.97f, 92.34f),
                        }),
                    })
                })
        {}
    }
}
