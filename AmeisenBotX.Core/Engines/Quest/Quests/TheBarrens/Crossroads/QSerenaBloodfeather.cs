using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Quest.Objects.Objectives;
using AmeisenBotX.Core.Engines.Quest.Objects.Quests;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Quest.Quests.TheBarrens.Crossroads
{
    internal class QSerenaBloodfeather : BotQuest
    {
        public QSerenaBloodfeather(AmeisenBotInterfaces bot)
            : base(bot, 876, "Serena Bloodfeather", 12, 1,
                () => (bot.GetClosestQuestGiverByNpcId(bot.Player.Position, new List<int> { 3449 }), new Vector3(-474.89f, -2607.74f, 127.89f)),
                () => (bot.GetClosestQuestGiverByNpcId(bot.Player.Position, new List<int> { 3449 }), new Vector3(-474.89f, -2607.74f, 127.89f)),
                [
                    new QuestObjectiveChain(
                    [
                        new KillAndLootQuestObjective(bot, [3452], 1, 5067, [
                            new()
                            {
                                new Vector3(790.37f, -1345.77f, 90.62f),
                            },
                        ]),
                    ])
                ])
        { }
    }
}