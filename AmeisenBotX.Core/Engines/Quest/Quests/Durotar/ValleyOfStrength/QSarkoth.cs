using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Quest.Objects.Objectives;
using AmeisenBotX.Core.Engines.Quest.Objects.Quests;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Quest.Quests.Durotar.ValleyOfStrength
{
    internal class QSarkoth : BotQuest
    {
        public QSarkoth(AmeisenBotInterfaces bot)
            : base(bot, 790, "Sarkoth", 1, 1,
                () => (bot.GetClosestQuestGiverByNpcId(bot.Player.Position, new List<int> { 3287 }), new Vector3(-397.76f, -4108.99f, 50.29f)),
                () => (bot.GetClosestQuestGiverByNpcId(bot.Player.Position, new List<int> { 3287 }), new Vector3(-397.76f, -4108.99f, 50.29f)),
                new List<IQuestObjective>()
                {
                    new QuestObjectiveChain(new List<IQuestObjective>()
                    {
                        new KillAndLootQuestObjective(bot, new List<int> { 3281 }, 1, 4905, new List<List<Vector3>> {
                            new()
                            {
                                new Vector3(-547.34f, -4103.85f, 70.10f),
                            },
                        }),
                    })
                })
        { }
    }
}