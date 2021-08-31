using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Quest.Objects.Objectives;
using AmeisenBotX.Core.Engines.Quest.Objects.Quests;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Quest.Quests.TheBarrens.Crossroads
{
    internal class QHarpyRaiders : BotQuest
    {
        public QHarpyRaiders(AmeisenBotInterfaces bot)
            : base(bot, 867, "Harpy Raiders", 12, 1,
                () => (bot.GetClosestQuestGiverByNpcId(bot.Player.Position, new List<int> { 3449 }), new Vector3(-474.89f, -2607.74f, 127.89f)),
                () => (bot.GetClosestQuestGiverByNpcId(bot.Player.Position, new List<int> { 3449 }), new Vector3(-474.89f, -2607.74f, 127.89f)),
                new List<IQuestObjective>()
                {
                    new QuestObjectiveChain(new List<IQuestObjective>()
                    {
                        new KillAndLootQuestObjective(bot, new List<int> { 3276,3277,3279,3280,3278,3452 }, 8, 5064, new List<List<Vector3>> {
                            new()
                            {
                                new Vector3(550.50f, -1152.00f, 91.79f),
                                new Vector3(518.00f, -1151.54f, 92.04f),
                                new Vector3(295.85f, -1443.80f, 91.79f),
                                new Vector3(281.38f, -1477.48f, 91.79f),
                                new Vector3(281.90f, -1545.01f, 91.79f),
                                new Vector3(306.95f, -1618.24f, 91.79f),
                                new Vector3(331.45f, -1621.73f, 92.17f),
                                new Vector3(617.57f, -1622.17f, 91.79f),
                                new Vector3(898.95f, -1352.25f, 92.92f),
                                new Vector3(905.31f, -1307.79f, 105.10f),
                                new Vector3(822.24f, -1261.96f, 107.57f),
                            },
                        }),
                    })
                })
        { }
    }
}