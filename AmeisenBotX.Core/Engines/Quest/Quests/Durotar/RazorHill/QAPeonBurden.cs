using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Quest.Objects.Quests;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Quest.Quests.Durotar.RazorHill
{
    internal class QAPeonBurden : BotQuest
    {
        public QAPeonBurden(AmeisenBotInterfaces bot)
            : base(bot, 2161, "A Peon's Burden", 1, 1,
                () => (bot.GetClosestQuestGiverByNpcId(bot.Player.Position, new List<int> { 6786 }), new Vector3(-599.45f, -4715.32f, 35.23f)),
                () => (bot.GetClosestQuestGiverByNpcId(bot.Player.Position, new List<int> { 6928 }), new Vector3(340.36f, -4686.29f, 16.54f)),
                null)
        { }
    }
}