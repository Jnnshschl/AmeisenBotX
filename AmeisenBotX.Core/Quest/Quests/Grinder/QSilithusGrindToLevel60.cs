using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Quest.Objects.Objectives;
using AmeisenBotX.Core.Quest.Objects.Quests;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Quest.Quests.Grinder
{
    internal class QSilithusGrindToLevel60 : GrindingBotQuest
    {
        public QSilithusGrindToLevel60(AmeisenBotInterfaces bot)
            : base("SilithusGrindToLevel60",
                new List<IQuestObjective>()
                {
                    new QuestObjectiveChain(new List<IQuestObjective>()
                    {
                        new GrindingObjective(bot, 60, new List<List<Vector3>> {
                            new()
                            {
                                new Vector3(-7183.99f, 488.21f, 13.89f),
                                new Vector3(-7249.29f, 489.92f, 11.61f),
                                new Vector3(-7314.79f, 414.78f, 21.74f),
                                new Vector3(-7316.36f, 386.53f, 15.65f),
                                new Vector3(-7315.56f, 309.60f, 10.91f),
                                new Vector3(-7309.82f, 284.82f, 12.22f),
                                new Vector3(-7117.15f, 241.95f, 3.21f),
                                new Vector3(-7084.70f, 241.67f, 3.86f),
                                new Vector3(-7054.74f, 276.02f, 5.81f),
                                new Vector3(-7055.20f, 319.81f, 7.36f),
                                new Vector3(-7085.28f, 390.38f, 5.02f),
                                new Vector3(-7122.72f, 435.07f, 18.60f),
                            },
                        }),
                    })
                })
        { }
    }
}