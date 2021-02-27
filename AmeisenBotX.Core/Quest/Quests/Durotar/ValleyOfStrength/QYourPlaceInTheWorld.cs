using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Core.Quest.Objects.Quests;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Quest.Quests.Durotar.ValleyOfStrength
{
    internal class QYourPlaceInTheWorld : BotQuest
    {
        public QYourPlaceInTheWorld(WowInterface wowInterface)
            : base(wowInterface, 4641, "Your Place In The World", 1, 1,
                () => (wowInterface.ObjectManager.GetClosestWowUnitByNpcId(new List<int> { 10176 }), new Vector3(-610.07f, -4253.52f, 39.04f)),
                () => (wowInterface.ObjectManager.GetClosestWowUnitByNpcId(new List<int> { 3143 }), new Vector3(-600.13f, -4186.19f, 41.27f)),
                null)
        { }
    }
}