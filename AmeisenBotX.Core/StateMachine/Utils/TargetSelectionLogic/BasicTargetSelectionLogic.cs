using AmeisenBotX.Core.Data.Objects.WowObjects;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Statemachine.Utils.TargetSelectionLogic
{
    public abstract class BasicTargetSelectionLogic
    {
        public IEnumerable<int> BlacklistedTargets { get; set; }

        public IEnumerable<int> PriorityTargets { get; set; }

        public void Reset()
        {
            BlacklistedTargets = null;
            PriorityTargets = null;
        }

        public abstract bool SelectTarget(out IEnumerable<WowUnit> wowUnit);

        protected bool IsBlacklisted(WowUnit wowUnit)
        {
            return BlacklistedTargets != null && BlacklistedTargets.Contains(wowUnit.DisplayId);
        }

        protected bool IsPriorityTarget(WowUnit wowUnit)
        {
            return PriorityTargets != null && PriorityTargets.Contains(wowUnit.DisplayId);
        }

        protected bool IsValidUnit(WowUnit wowUnit)
        {
            return !wowUnit.IsDead
                && !wowUnit.IsNotAttackable
                && wowUnit.IsInCombat
                && !IsBlacklisted(wowUnit)
                && !wowUnit.IsFriendyTo(WowInterface.I.Player)
                && wowUnit.DistanceTo(WowInterface.I.Player) < 80.0f;
        }
    }
}