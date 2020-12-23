using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Core.Quest.Objects.Objectives;
using AmeisenBotX.Core.Quest.Objects.Quests;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Quest.Quests.TheBarrens.OutpostBridge
{
    class QCrossroadsConscription : BotQuest
    {
        public QCrossroadsConscription(WowInterface wowInterface)
            : base(wowInterface, 842, "Crossroads Conscription", 10, 1,
                () => (wowInterface.ObjectManager.GetClosestWowUnitByNpcId(new List<int> { 3337 }), new Vector3(303.43f, -3686.16f, 27.15f)),
                () => (wowInterface.ObjectManager.GetClosestWowUnitByNpcId(new List<int> { 3338 }), new Vector3(-482.48f, -2670.19f, 97.52f)),
                null)
        {}
    }
}
