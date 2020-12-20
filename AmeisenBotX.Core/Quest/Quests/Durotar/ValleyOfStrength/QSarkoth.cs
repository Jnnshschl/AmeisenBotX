using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Core.Quest.Objects.Objectives;
using AmeisenBotX.Core.Quest.Objects.Quests;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Quest.Quests.Durotar.ValleyOfStrength
{
    class QSarkoth : BotQuest
    {
        public QSarkoth(WowInterface wowInterface)
            : base(wowInterface, 790, "Sarkoth", 1, 1,
                () => (wowInterface.ObjectManager.GetClosestWowUnitByNpcId(new List<int> { 3287 }), new Vector3(-397.76f, -4108.99f, 50.29f)),
                () => (wowInterface.ObjectManager.GetClosestWowUnitByNpcId(new List<int> { 3287 }), new Vector3(-397.76f, -4108.99f, 50.29f)),
                new List<IQuestObjective>()
                {
                    new QuestObjectiveChain(new List<IQuestObjective>()
                    {
                        new KillAndLootQuestObjective(wowInterface, new List<int> { 3281 }, 1, 4905, new List<List<Vector3>> {
                            new()
                            {
                                new Vector3(-547.34f, -4103.85f, 70.10f),
                            },
                        }),
                    })
                })
        {}
    }
}
