using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Quest.Objects.Objectives;
using AmeisenBotX.Core.Engines.Quest.Objects.Quests;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Quest.Quests.Durotar.ValleyOfStrength
{
    internal class QStingOfTheScorpid : BotQuest
    {
        public QStingOfTheScorpid(AmeisenBotInterfaces bot)
            : base(bot, 789, "Sting of the Scorpid", 1, 1,
                () => (bot.GetClosestQuestGiverByNpcId(bot.Player.Position, new List<int> { 3143 }), new Vector3(-600.13f, -4186.19f, 41.27f)),
                () => (bot.GetClosestQuestGiverByNpcId(bot.Player.Position, new List<int> { 3143 }), new Vector3(-600.13f, -4186.19f, 41.27f)),
                [
                    new QuestObjectiveChain(
                    [
                        new KillAndLootQuestObjective(bot, [3124, 3281], 8, 4862, [
                            new()
                            {
                                new Vector3(-756.20f, -4352.79f, 52.21f),
                            },
                            new()
                            {
                                new Vector3(-787.39f, -4246.54f, 52.66f),
                            },
                            new()
                            {
                                new Vector3(-320.45f, -4153.63f, 53.29f),
                                new Vector3(-346.63f, -4045.22f, 51.11f),
                                new Vector3(-610.69f, -4084.09f, 77.46f),
                                new Vector3(-676.94f, -4114.47f, 39.30f),
                                new Vector3(-680.31f, -4145.69f, 36.07f),
                                new Vector3(-511.39f, -4174.16f, 77.30f),
                            },
                            new()
                            {
                                new Vector3(-788.54f, -4286.46f, 52.82f),
                            },
                            new()
                            {
                                new Vector3(-707.63f, -4346.49f, 47.97f),
                            },
                            new()
                            {
                                new Vector3(-180.70f, -4243.65f, 56.93f),
                                new Vector3(-212.50f, -4170.83f, 64.59f),
                                new Vector3(-248.87f, -4181.38f, 55.91f),
                                new Vector3(-279.49f, -4214.26f, 55.28f),
                                new Vector3(-448.26f, -4459.52f, 50.28f),
                                new Vector3(-448.05f, -4486.25f, 54.76f),
                                new Vector3(-304.94f, -4437.49f, 59.06f),
                                new Vector3(-282.79f, -4416.63f, 56.68f),
                            },
                        ]),
                    ])
                ])
        { }
    }
}