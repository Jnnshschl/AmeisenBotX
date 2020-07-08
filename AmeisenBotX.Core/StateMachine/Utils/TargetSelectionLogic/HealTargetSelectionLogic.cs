using AmeisenBotX.Core.Data.Objects.WowObject;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Statemachine.Utils.TargetSelectionLogic
{
    public class HealTargetSelectionLogic : ITargetSelectionLogic
    {
        public HealTargetSelectionLogic(WowInterface wowInterface, int healthThreshold = 90, bool groupOnly = true)
        {
            WowInterface = wowInterface;
            HealthThreshold = healthThreshold;
            GroupOnly = groupOnly;
        }

        public bool GroupOnly { get; set; }

        public int HealthThreshold { get; set; }

        public List<string> PriorityTargets { get; set; }

        private WowInterface WowInterface { get; }

        public void Reset()
        {
        }

        public bool SelectTarget(out List<WowUnit> possibleTargets)
        {
            if (NeedToHealSomeone(out possibleTargets))
            {
                // select the one with lowest hp
                possibleTargets = possibleTargets.OrderBy(e => e.HealthPercentage).ToList();
                return true;
            }

            possibleTargets = null;
            return false;
        }

        private bool NeedToHealSomeone(out List<WowUnit> playersThatNeedHealing)
        {
            List<WowPlayer> groupPlayers = WowInterface.ObjectManager.WowObjects.OfType<WowPlayer>().Where(e => !e.IsDead
                && e.Health > 1
                && WowInterface.HookManager.GetUnitReaction(WowInterface.ObjectManager.Player, e) == WowUnitReaction.Friendly
                && (!GroupOnly || WowInterface.ObjectManager.PartymemberGuids.Contains(e.Guid))).ToList();

            groupPlayers.Add(WowInterface.ObjectManager.Player);

            playersThatNeedHealing = groupPlayers.Where(e => e.HealthPercentage < HealthThreshold).OfType<WowUnit>().ToList();
            return playersThatNeedHealing.Count > 0;
        }
    }
}