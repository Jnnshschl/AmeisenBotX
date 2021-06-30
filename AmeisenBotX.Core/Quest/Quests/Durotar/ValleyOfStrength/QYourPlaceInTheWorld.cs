using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Quest.Objects.Quests;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Quest.Quests.Durotar.ValleyOfStrength
{
    internal class QYourPlaceInTheWorld : BotQuest
    {
        public QYourPlaceInTheWorld(AmeisenBotInterfaces bot)
            : base(bot, 4641, "Your Place In The World", 1, 1,
                () => (bot.Objects.GetClosestWowUnitByNpcId(bot.Player.Position, new List<int> { 10176 }), new Vector3(-610.07f, -4253.52f, 39.04f)),
                () => (bot.Objects.GetClosestWowUnitByNpcId(bot.Player.Position, new List<int> { 3143 }), new Vector3(-600.13f, -4186.19f, 41.27f)),
                null)
        { }
    }
}