using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Quest.Objects.Quests;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Quest.Quests.TheBarrens.Crossroads
{
    internal class QLetterToJinZil : BotQuest
    {
        public QLetterToJinZil(AmeisenBotInterfaces bot)
            : base(bot, 1060, "Letter to Jin'Zil", 15, 1,
                () => (bot.GetClosestQuestGiverByNpcId(bot.Player.Position, new List<int> { 3449 }), new Vector3(-474.89f, -2607.74f, 127.89f)),
                () => (bot.GetClosestQuestGiverByNpcId(bot.Player.Position, new List<int> { 3995 }), new Vector3(-272.48f, -394.08f, 17.21f)),
                null)
        { }
    }
}