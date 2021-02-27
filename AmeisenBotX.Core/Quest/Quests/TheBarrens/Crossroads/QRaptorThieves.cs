using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Core.Quest.Objects.Objectives;
using AmeisenBotX.Core.Quest.Objects.Quests;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Quest.Quests.TheBarrens.Crossroads
{
    internal class QRaptorThieves : BotQuest
    {
        public QRaptorThieves(WowInterface wowInterface)
            : base(wowInterface, 869, "Raptor Thieves", 9, 1,
                () => (wowInterface.ObjectManager.GetClosestWowUnitByNpcId(new List<int> { 3464 }), new Vector3(-435.95f, -2639.21f, 96.36f)),
                () => (wowInterface.ObjectManager.GetClosestWowUnitByNpcId(new List<int> { 3464 }), new Vector3(-435.95f, -2639.21f, 96.36f)),
                new List<IQuestObjective>()
                {
                    new QuestObjectiveChain(new List<IQuestObjective>()
                    {
                        new KillAndLootQuestObjective(wowInterface, new List<int> { 3254,3255,3256,3257,5842 }, 12, 5062, new List<List<Vector3>> {
                            new()
                            {
                                new Vector3(-332.38f, -1044.35f, 41.14f),
                                new Vector3(-1322.19f, -1823.66f, 91.93f),
                                new Vector3(-1783.92f, -2250.27f, 91.74f),
                                new Vector3(-1844.17f, -2387.86f, 96.92f),
                                new Vector3(-1887.46f, -2512.17f, 91.79f),
                                new Vector3(-2068.89f, -3151.03f, 117.67f),
                                new Vector3(-2035.41f, -3255.18f, 91.86f),
                                new Vector3(-1767.16f, -3547.77f, 93.76f),
                                new Vector3(-1554.88f, -3629.48f, 128.51f),
                                new Vector3(1204.33f, -3821.24f, 28.62f),
                                new Vector3(1318.61f, -3116.13f, 91.97f),
                                new Vector3(555.39f, -1688.75f, 92.54f),
                                new Vector3(-172.88f, -1091.55f, 46.78f),
                            },
                        }),
                    })
                })
        { }
    }
}