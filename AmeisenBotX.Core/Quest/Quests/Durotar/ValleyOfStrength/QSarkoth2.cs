using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Quest.Objects.Quests;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Quest.Quests.Durotar.ValleyOfStrength
{
    internal class QSarkoth2 : BotQuest
    {
        public QSarkoth2(WowInterface wowInterface)
            : base(wowInterface, 804, "Sarkoth", 1, 1,
                () => (wowInterface.Objects.GetClosestWowUnitByNpcId(wowInterface.Player.Position, new List<int> { 3287 }), new Vector3(-397.76f, -4108.99f, 50.29f)),
                () => (wowInterface.Objects.GetClosestWowUnitByNpcId(wowInterface.Player.Position, new List<int> { 3143 }), new Vector3(-600.13f, -4186.19f, 41.27f)),
                null)
        { }
    }
}