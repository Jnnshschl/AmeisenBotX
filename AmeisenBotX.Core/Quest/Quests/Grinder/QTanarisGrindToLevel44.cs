using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Quest.Objects.Objectives;
using AmeisenBotX.Core.Quest.Objects.Quests;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Quest.Quests.Grinder
{
    internal class QTanarisGrindToLevel44 : GrindingBotQuest
    {
        public QTanarisGrindToLevel44(AmeisenBotInterfaces bot)
            : base("TanarisGrindToLevel44",
                new List<IQuestObjective>()
                {
                    new QuestObjectiveChain(new List<IQuestObjective>()
                    {
                        new GrindingObjective(bot, 44, new List<List<Vector3>> {
                            new()
                            {
                                new Vector3(-7304.35f, -4604.55f, 8.49f),
                                new Vector3(-7319.44f, -4618.22f, 8.86f),
                                new Vector3(-7311.40f, -4627.60f, 8.87f),
                                new Vector3(-7305.61f, -4621.26f, 8.86f),
                                new Vector3(-7299.00f, -4607.96f, 8.56f),
                            },
                            new()
                            {
                                new Vector3(-7011.46f, -4317.52f, 10.67f),
                                new Vector3(-7014.06f, -4387.13f, 9.36f),
                                new Vector3(-6987.99f, -4413.14f, 9.65f),
                                new Vector3(-6955.21f, -4417.71f, 11.08f),
                                new Vector3(-6919.89f, -4413.27f, 11.34f),
                                new Vector3(-6919.79f, -4380.21f, 11.59f),
                                new Vector3(-6922.39f, -4349.26f, 11.22f),
                            },
                            new()
                            {
                                new Vector3(-6952.23f, -4290.61f, 9.42f),
                            },
                            new()
                            {
                                new Vector3(-7275.84f, -4542.53f, 9.00f),
                                new Vector3(-7291.17f, -4554.87f, 9.63f),
                                new Vector3(-7257.62f, -4544.47f, 9.15f),
                            },
                            new()
                            {
                                new Vector3(-7410.04f, -4548.58f, 10.82f),
                                new Vector3(-7444.27f, -4606.74f, 10.63f),
                                new Vector3(-7411.57f, -4616.26f, 10.98f),
                                new Vector3(-7381.99f, -4608.66f, 9.59f),
                                new Vector3(-7382.23f, -4590.90f, 9.15f),
                                new Vector3(-7387.71f, -4557.04f, 11.07f),
                            },
                            new()
                            {
                                new Vector3(-7386.43f, -4729.27f, 9.24f),
                                new Vector3(-7351.86f, -4746.29f, 9.96f),
                                new Vector3(-7378.89f, -4712.70f, 10.99f),
                                new Vector3(-7408.62f, -4712.06f, 9.20f),
                            },
                            new()
                            {
                                new Vector3(-7225.02f, -4593.96f, 9.00f),
                                new Vector3(-7244.73f, -4620.39f, 8.70f),
                                new Vector3(-7236.40f, -4631.24f, 9.30f),
                                new Vector3(-7218.59f, -4645.01f, 9.25f),
                                new Vector3(-7213.31f, -4629.97f, 8.98f),
                                new Vector3(-7215.89f, -4617.47f, 8.97f),
                            },
                        }),
                    })
                })
        { }
    }
}