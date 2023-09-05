using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Quest.Objects.Objectives;
using AmeisenBotX.Core.Engines.Quest.Objects.Quests;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Quest.Quests.Durotar.RazorHill
{
    internal class QVanquishTheBetrayers : BotQuest
    {
        public QVanquishTheBetrayers(AmeisenBotInterfaces bot)
            : base(bot, 784, "Vanquish the Betrayers", 3, 1,
                () => (bot.GetClosestQuestGiverByNpcId(bot.Player.Position, new List<int> { 3139 }), new Vector3(274.99f, -4709.30f, 17.57f)),
                () => (bot.GetClosestQuestGiverByNpcId(bot.Player.Position, new List<int> { 3139 }), new Vector3(274.99f, -4709.30f, 17.57f)),
                () => new List<IQuestObjective>()
                {
                    new QuestObjectiveChain(new List<IQuestObjective>()
                    {
                        new KillAndLootQuestObjective(bot, new List<int> { 3128 }, 10, 0, new List<List<Vector3>> {
                            new()
                            {
                                new Vector3(-220.08f, -4908.83f, 26.92f),
                                new Vector3(-273.24f, -4919.18f, 27.19f),
                                new Vector3(-312.36f, -5070.01f, 21.63f),
                                new Vector3(-262.55f, -5188.37f, 21.15f),
                                new Vector3(-48.03f, -5080.86f, 10.22f),
                                new Vector3(-7.66f, -4982.83f, 13.48f),
                                new Vector3(-18.87f, -4917.26f, 16.75f),
                            },
                        }),
                        new KillAndLootQuestObjective(bot, new List<int> { 3129 }, 8, 0, new List<List<Vector3>> {
                            new()
                            {
                                new Vector3(17.51f, -4951.48f, 14.39f),
                            },
                            new()
                            {
                                new Vector3(-141.46f, -5003.08f, 22.19f),
                                new Vector3(-253.49f, -5052.29f, 21.32f),
                                new Vector3(-312.70f, -5145.91f, 21.43f),
                                new Vector3(-284.44f, -5174.54f, 21.28f),
                                new Vector3(-120.03f, -5131.44f, 21.60f),
                                new Vector3(-78.06f, -5110.51f, 17.10f),
                                new Vector3(-87.91f, -5018.34f, 16.56f),
                            },
                        }),
                        new KillAndLootQuestObjective(bot, new List<int> { 3192 }, 1, 0, new List<List<Vector3>> {
                            new()
                            {
                                new Vector3(-245.52f, -5119.95f, 42.64f),
                            },
                        }),
                    })
                })
        { }
    }
}