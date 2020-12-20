using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Core.Quest.Objects.Objectives;
using AmeisenBotX.Core.Quest.Objects.Quests;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Quest.Quests.Durotar.ValleyOfStrength
{
    class QGalgarCactusAppleSurprise : BotQuest
    {
        public QGalgarCactusAppleSurprise(WowInterface wowInterface)
            : base(wowInterface, 4402, "Galgar's Cactus Apple Surprise", 1, 1,
                () => (wowInterface.ObjectManager.GetClosestWowUnitByNpcId(new List<int> { 9796 }), new Vector3(-561.63f, -4221.80f, 41.67f)),
                () => (wowInterface.ObjectManager.GetClosestWowUnitByNpcId(new List<int> { 9796 }), new Vector3(-561.63f, -4221.80f, 41.67f)),
                new List<IQuestObjective>()
                {
                    new QuestObjectiveChain(new List<IQuestObjective>()
                    {
                        new CollectQuestObjective(wowInterface, 11583, 6, new List<int> { 171938 }, new List<Vector3> {
                            new Vector3(-489.09f, -4301.17f, 42.87f),
                            new Vector3(-406.27f, -4279.20f, 46.38f),
                            new Vector3(-487.61f, -4277.06f, 43.01f),
                            new Vector3(-326.03f, -4395.06f, 58.33f),
                            new Vector3(-486.92f, -4291.01f, 43.22f),
                            new Vector3(-404.26f, -4263.58f, 49.38f),
                            new Vector3(-360.68f, -4337.94f, 58.19f),
                            new Vector3(-556.42f, -4288.72f, 37.44f),
                            new Vector3(-422.73f, -4377.85f, 42.23f),
                            new Vector3(-551.58f, -4292.26f, 37.09f),
                            new Vector3(-592.09f, -4074.48f, 74.47f),
                            new Vector3(-444.98f, -4122.43f, 51.09f),
                            new Vector3(-244.93f, -4318.93f, 61.35f),
                            new Vector3(-295.80f, -4337.23f, 56.83f),
                            new Vector3(-465.10f, -4381.39f, 50.60f),
                            new Vector3(-406.41f, -4460.83f, 51.98f),
                            new Vector3(-423.57f, -4175.42f, 50.84f),
                            new Vector3(-516.89f, -4187.04f, 77.14f),
                            new Vector3(-746.49f, -4276.66f, 43.77f),
                            new Vector3(-422.69f, -4187.55f, 51.65f),
                            new Vector3(-469.63f, -4378.43f, 48.38f),
                            new Vector3(-413.09f, -4398.37f, 43.59f),
                            new Vector3(-407.97f, -4061.42f, 51.86f),
                            new Vector3(-317.47f, -4438.41f, 57.44f),
                            new Vector3(-674.29f, -4300.25f, 44.95f),
                            new Vector3(-427.71f, -4185.25f, 50.45f),
                            new Vector3(-601.58f, -4075.62f, 75.86f),
                            new Vector3(-322.21f, -4438.69f, 56.77f),
                            new Vector3(-408.21f, -4395.52f, 42.77f),
                            new Vector3(-317.60f, -4105.12f, 54.33f),
                            new Vector3(-489.68f, -4089.52f, 64.56f),
                            new Vector3(-182.64f, -4183.35f, 81.11f),
                            new Vector3(-489.84f, -4464.14f, 51.98f),
                            new Vector3(-330.87f, -4393.10f, 58.53f),
                            new Vector3(-298.44f, -4332.24f, 56.60f),
                            new Vector3(-482.64f, -4083.92f, 65.42f),
                            new Vector3(-183.50f, -4181.80f, 80.82f),
                            new Vector3(-261.12f, -4159.08f, 54.93f),
                            new Vector3(-255.70f, -4160.31f, 55.86f),
                            new Vector3(-475.43f, -4323.95f, 44.02f),
                            new Vector3(-260.52f, -4211.04f, 58.73f),
                            new Vector3(-523.08f, -4182.39f, 76.94f),
                            new Vector3(-413.36f, -4058.05f, 52.31f),
                            new Vector3(-364.98f, -4333.73f, 55.19f),
                            new Vector3(-696.94f, -4355.80f, 54.18f),
                        }),
                    })
                })
        {}
    }
}
