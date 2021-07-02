using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Quest.Objects.Objectives;
using AmeisenBotX.Core.Engines.Quest.Objects.Quests;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Quest.Quests.Grinder
{
    internal class QStonetalonGrindToLevel23 : GrindingBotQuest
    {
        public QStonetalonGrindToLevel23(AmeisenBotInterfaces bot)
            : base("StonetalonGrindToLevel23",
                new List<IQuestObjective>()
                {
                    new QuestObjectiveChain(new List<IQuestObjective>()
                    {
                        new GrindingObjective(bot, 23, new List<List<Vector3>> {
                            new()
                            {
                                new Vector3(1414.59f, 164.04f, 18.81f),
                                new Vector3(1215.00f, 343.34f, 32.46f),
                                new Vector3(1128.53f, 283.56f, 16.99f),
                                new Vector3(1045.58f, 112.17f, 16.00f),
                                new Vector3(1022.11f, 23.91f, 14.59f),
                                new Vector3(1056.71f, -217.82f, 4.36f),
                                new Vector3(1120.26f, -422.30f, 13.70f),
                                new Vector3(1363.80f, -249.66f, -2.55f),
                                new Vector3(1447.59f, -27.98f, 27.25f),
                                new Vector3(1454.69f, 78.88f, 18.39f),
                            },
                            new()
                            {
                                new Vector3(993.42f, -382.99f, 8.45f),
                                new Vector3(973.44f, -385.42f, 8.26f),
                                new Vector3(988.42f, -421.82f, 7.94f),
                                new Vector3(1006.76f, -394.96f, 7.06f),
                            },
                            new()
                            {
                                new Vector3(1481.77f, -222.40f, 23.03f),
                                new Vector3(1481.04f, -249.44f, 23.85f),
                                new Vector3(1510.72f, -251.47f, 30.71f),
                            },
                        }),
                    })
                })
        { }
    }
}