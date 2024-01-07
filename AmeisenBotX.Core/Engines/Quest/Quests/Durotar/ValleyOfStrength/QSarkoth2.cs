using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Quest.Objects.Quests;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Quest.Quests.Durotar.ValleyOfStrength
{
    internal class QSarkoth2(AmeisenBotInterfaces bot) : BotQuest(bot, 804, "Sarkoth", 1, 1,
            () => (bot.GetClosestQuestGiverByNpcId(bot.Player.Position, new List<int> { 3287 }), new Vector3(-397.76f, -4108.99f, 50.29f)),
            () => (bot.GetClosestQuestGiverByNpcId(bot.Player.Position, new List<int> { 3143 }), new Vector3(-600.13f, -4186.19f, 41.27f)),
            null)
    {
    }
}