using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Quest.Objects.Objectives;
using AmeisenBotX.Core.Engines.Quest.Objects.Quests;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Quest.Quests.Durotar.RazorHill
{
    internal class QCarryYourWeight : BotQuest
    {
        public QCarryYourWeight(AmeisenBotInterfaces bot)
            : base(bot, 791, "Carry Your Weight", 4, 1,
                () => (bot.GetClosestQuestgiverByNpcId(bot.Player.Position, new List<int> { 3147 }), new Vector3(384.74f, -4600.13f, 76.17f)),
                () => (bot.GetClosestQuestgiverByNpcId(bot.Player.Position, new List<int> { 3147 }), new Vector3(384.74f, -4600.13f, 76.17f)),
                new List<IQuestObjective>()
                {
                    new QuestObjectiveChain(new List<IQuestObjective>()
                    {
                        new KillAndLootQuestObjective(bot, new List<int> { 3119,3120,3128,3129,3192,5808,5809 }, 8, 4870, new List<List<Vector3>> {
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
                            new()
                            {
                                new Vector3(-918.93f, -4494.03f, 29.65f),
                                new Vector3(-986.97f, -4410.00f, 29.37f),
                                new Vector3(-1030.65f, -4432.20f, 26.16f),
                                new Vector3(-1083.06f, -4716.26f, 15.57f),
                                new Vector3(-1054.21f, -4745.53f, 16.66f),
                                new Vector3(-1005.58f, -4766.97f, 12.67f),
                            },
                        }),
                    })
                })
        { }
    }
}