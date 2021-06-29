using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Quest.Objects.Quests;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Quest.Quests.TheBarrens.OutpostBridge
{
    internal class QCrossroadsConscription : BotQuest
    {
        public QCrossroadsConscription(WowInterface wowInterface)
            : base(wowInterface, 842, "Crossroads Conscription", 10, 1,
                () => (wowInterface.Objects.GetClosestWowUnitByNpcId(wowInterface.Player.Position, new List<int> { 3337 }), new Vector3(303.43f, -3686.16f, 27.15f)),
                () => (wowInterface.Objects.GetClosestWowUnitByNpcId(wowInterface.Player.Position, new List<int> { 3338 }), new Vector3(-482.48f, -2670.19f, 97.52f)),
                null)
        { }
    }
}