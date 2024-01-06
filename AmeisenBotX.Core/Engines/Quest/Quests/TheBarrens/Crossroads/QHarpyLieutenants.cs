using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Quest.Objects.Objectives;
using AmeisenBotX.Core.Engines.Quest.Objects.Quests;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Quest.Quests.TheBarrens.Crossroads
{
    internal class QHarpyLieutenants : BotQuest
    {
        public QHarpyLieutenants(AmeisenBotInterfaces bot)
            : base(bot, 875, "Harpy Lieutenants", 12, 1,
                () => (bot.GetClosestQuestGiverByNpcId(bot.Player.Position, new List<int> { 3449 }), new Vector3(-474.89f, -2607.74f, 127.89f)),
                () => (bot.GetClosestQuestGiverByNpcId(bot.Player.Position, new List<int> { 3449 }), new Vector3(-474.89f, -2607.74f, 127.89f)),
                [
                    new QuestObjectiveChain(
                    [
                        new KillAndLootQuestObjective(bot, [3278], 6, 5065, [
                            new()
                            {
                                new Vector3(518.00f, -1151.54f, 92.04f),
                                new Vector3(428.48f, -1277.83f, 92.17f),
                                new Vector3(420.40f, -1310.91f, 92.79f),
                                new Vector3(479.85f, -1412.66f, 91.79f),
                                new Vector3(533.50f, -1447.21f, 91.75f),
                                new Vector3(568.88f, -1460.79f, 92.67f),
                                new Vector3(808.86f, -1422.43f, 95.42f),
                                new Vector3(898.95f, -1352.25f, 92.92f),
                                new Vector3(891.89f, -1308.38f, 103.54f),
                                new Vector3(762.79f, -1246.36f, 91.92f),
                                new Vector3(550.50f, -1152.00f, 91.79f),
                            },
                        ]),
                    ])
                ])
        { }
    }
}