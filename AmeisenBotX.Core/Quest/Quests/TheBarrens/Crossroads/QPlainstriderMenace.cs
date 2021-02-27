using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Core.Quest.Objects.Objectives;
using AmeisenBotX.Core.Quest.Objects.Quests;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Quest.Quests.TheBarrens.Crossroads
{
    internal class QPlainstriderMenace : BotQuest
    {
        public QPlainstriderMenace(WowInterface wowInterface)
            : base(wowInterface, 844, "Plainstrider Menace", 10, 1,
                () => (wowInterface.ObjectManager.GetClosestWowUnitByNpcId(new List<int> { 3338 }), new Vector3(-482.48f, -2670.19f, 97.52f)),
                () => (wowInterface.ObjectManager.GetClosestWowUnitByNpcId(new List<int> { 3338 }), new Vector3(-482.48f, -2670.19f, 97.52f)),
                new List<IQuestObjective>()
                {
                    new QuestObjectiveChain(new List<IQuestObjective>()
                    {
                        new KillAndLootQuestObjective(wowInterface, new List<int> { 3244,3245,3246 }, 7, 5087, new List<List<Vector3>> {
                            new()
                            {
                                new Vector3(-306.33f, -1152.27f, 58.90f),
                                new Vector3(-1754.28f, -2317.78f, 92.61f),
                                new Vector3(-1926.47f, -2869.90f, 91.94f),
                                new Vector3(-1944.01f, -2949.78f, 93.42f),
                                new Vector3(-1992.24f, -3413.41f, 56.07f),
                                new Vector3(-1714.30f, -3656.39f, 79.90f),
                                new Vector3(-790.42f, -3859.12f, 13.47f),
                                new Vector3(-558.62f, -3856.55f, 28.71f),
                                new Vector3(-420.50f, -3854.07f, 27.50f),
                                new Vector3(719.14f, -3595.32f, 90.80f),
                                new Vector3(952.51f, -3536.90f, 94.10f),
                                new Vector3(1156.88f, -3365.88f, 91.81f),
                                new Vector3(1251.53f, -3053.21f, 91.79f),
                                new Vector3(578.88f, -1642.39f, 91.84f),
                            },
                        }),
                    })
                })
        { }
    }
}