using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Utils.TargetSelection
{
    public abstract class BasicTargetSelectionLogic
    {
        public BasicTargetSelectionLogic(WowInterface wowInterface)
        {
            WowInterface = wowInterface;
        }

        public IEnumerable<int> BlacklistedTargets { get; set; }

        public IEnumerable<int> PriorityTargets { get; set; }

        public WowInterface WowInterface { get; }

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
                && WowInterface.NewWowInterface.GetReaction(wowUnit.BaseAddress, WowInterface.Player.BaseAddress) == WowUnitReaction.Hostile
                && wowUnit.DistanceTo(WowInterface.Player) < 80.0f;
        }
    }
}