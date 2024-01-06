using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Quest.Objects.Objectives;
using AmeisenBotX.Core.Engines.Quest.Objects.Quests;

namespace AmeisenBotX.Core.Engines.Quest.Quests.Grinder
{
    internal class QDurotarGrindToLevel9 : GrindingBotQuest
    {
        public QDurotarGrindToLevel9(AmeisenBotInterfaces bot)
            : base("DurotarGrindToLevel9",
                [
                    new QuestObjectiveChain(
                    [
                        new GrindingObjective(bot, 9, [
                            new()
                            {
                                new Vector3(-220.08f, -4908.83f, 26.92f),
                                new Vector3(-273.24f, -4919.18f, 27.19f),
                                new Vector3(-312.36f, -5070.01f, 21.63f),
                                new Vector3(-312.70f, -5145.91f, 21.43f),
                                new Vector3(-284.44f, -5174.54f, 21.28f),
                                new Vector3(-262.55f, -5188.37f, 21.15f),
                                new Vector3(-120.03f, -5131.44f, 21.60f),
                                new Vector3(-78.06f, -5110.51f, 17.10f),
                                new Vector3(-48.03f, -5080.86f, 10.22f),
                                new Vector3(17.51f, -4951.48f, 14.39f),
                                new Vector3(-18.87f, -4917.26f, 16.75f),
                            },
                        ]),
                    ])
                ])
        { }
    }
}