using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Quest.Objects.Quests;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Quest.Quests.TheBarrens.OutpostBridge
{
    internal class QCrossroadsConscription : BotQuest
    {
        public QCrossroadsConscription(AmeisenBotInterfaces bot)
            : base(bot, 842, "Crossroads Conscription", 10, 1,
                () => (bot.GetClosestQuestgiverByNpcId(bot.Player.Position, new List<int> { 3337 }), new Vector3(303.43f, -3686.16f, 27.15f)),
                () => (bot.GetClosestQuestgiverByNpcId(bot.Player.Position, new List<int> { 3338 }), new Vector3(-482.48f, -2670.19f, 97.52f)),
                null)
        { }
    }
}