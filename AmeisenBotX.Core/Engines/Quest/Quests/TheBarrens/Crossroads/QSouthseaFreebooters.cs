using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Quest.Objects.Objectives;
using AmeisenBotX.Core.Engines.Quest.Objects.Quests;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Quest.Quests.TheBarrens.Crossroads
{
    internal class QSouthseaFreebooters : BotQuest
    {
        public QSouthseaFreebooters(AmeisenBotInterfaces bot)
            : base(bot, 887, "Southsea Freebooters", 9, 1,
                () => (bot.GetClosestQuestgiverByNpcId(bot.Player.Position, new List<int> { 3391 }), new Vector3(-835.56f, -3728.66f, 26.37f)),
                () => (bot.GetClosestQuestgiverByNpcId(bot.Player.Position, new List<int> { 3391 }), new Vector3(-835.56f, -3728.66f, 26.37f)),
                new List<IQuestObjective>()
                {
                    new QuestObjectiveChain(new List<IQuestObjective>()
                    {
                        new KillAndLootQuestObjective(bot, new List<int> { 3381 }, 12, 0, new List<List<Vector3>> {
                            new()
                            {
                                new Vector3(-1742.90f, -3730.12f, 13.76f),
                                new Vector3(-1779.11f, -3746.11f, 6.99f),
                                new Vector3(-1710.46f, -3853.85f, 9.37f),
                                new Vector3(-1543.04f, -3905.79f, 13.83f),
                                new Vector3(-1355.43f, -3909.21f, 9.09f),
                                new Vector3(-1322.96f, -3884.70f, 11.28f),
                                new Vector3(-1316.59f, -3819.80f, 18.28f),
                            },
                        }),
                        new KillAndLootQuestObjective(bot, new List<int> { 3382 }, 6, 0, new List<List<Vector3>> {
                            new()
                            {
                                new Vector3(-1731.59f, -3715.31f, 16.97f),
                                new Vector3(-1778.44f, -3724.77f, 10.15f),
                                new Vector3(-1729.21f, -3839.87f, 10.49f),
                                new Vector3(-1563.96f, -3906.81f, 12.58f),
                                new Vector3(-1364.40f, -3922.69f, 10.46f),
                                new Vector3(-1348.52f, -3848.37f, 17.94f),
                                new Vector3(-1362.63f, -3753.26f, 59.90f),
                            },
                        }),
                    })
                })
        { }
    }
}