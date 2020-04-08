using AmeisenBotX.Core.Data.Objects.WowObject;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Statemachine.Utils.TargetSelectionLogic
{
    public class HealTargetSelectionLogic : ITargetSelectionLogic
    {
        public HealTargetSelectionLogic(WowInterface wowInterface)
        {
            WowInterface = wowInterface;
        }

        private WowInterface WowInterface { get; }

        public void Reset()
        {
        }

        public bool SelectTarget(out List<WowUnit> targetToSelect)
        {
            if (NeedToHealSomeone(out targetToSelect))
            {
                // select the one with lowest hp
                targetToSelect = targetToSelect.OrderBy(e => e.HealthPercentage).ToList();
                return true;
            }

            targetToSelect = null;
            return false;
        }

        private bool NeedToHealSomeone(out List<WowUnit> playersThatNeedHealing)
        {
            IEnumerable<WowPlayer> players = WowInterface.ObjectManager.WowObjects.OfType<WowPlayer>();
            List<WowPlayer> groupPlayers = players.Where(e => !e.IsDead && e.Health > 1 && WowInterface.ObjectManager.PartymemberGuids.Contains(e.Guid)).ToList();

            groupPlayers.Add(WowInterface.ObjectManager.Player);

            playersThatNeedHealing = groupPlayers.Where(e => e.HealthPercentage < 90).OfType<WowUnit>().ToList();

            return playersThatNeedHealing.Count > 0;
        }
    }
}