using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Quest.Objects.Quests;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Quest.Quests.Durotar.ValleyOfStrength
{
    internal class QYourPlaceInTheWorld(AmeisenBotInterfaces bot) : BotQuest(bot, 4641, "Your Place In The World", 1, 1,
            () => (bot.GetClosestQuestGiverByNpcId(bot.Player.Position, new List<int> { 10176 }), new Vector3(-610.07f, -4253.52f, 39.04f)),
            () => (bot.GetClosestQuestGiverByNpcId(bot.Player.Position, new List<int> { 3143 }), new Vector3(-600.13f, -4186.19f, 41.27f)),
            null)
    {
    }
}