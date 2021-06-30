using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Quest.Objects.Quests;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Quest.Quests.Durotar.RazorHill
{
    internal class QAPeonBurden : BotQuest
    {
        public QAPeonBurden(AmeisenBotInterfaces bot)
            : base(bot, 2161, "A Peon's Burden", 1, 1,
                () => (bot.Objects.GetClosestWowUnitByNpcId(bot.Player.Position, new List<int> { 6786 }), new Vector3(-599.45f, -4715.32f, 35.23f)),
                () => (bot.Objects.GetClosestWowUnitByNpcId(bot.Player.Position, new List<int> { 6928 }), new Vector3(340.36f, -4686.29f, 16.54f)),
                null)
        { }
    }
}