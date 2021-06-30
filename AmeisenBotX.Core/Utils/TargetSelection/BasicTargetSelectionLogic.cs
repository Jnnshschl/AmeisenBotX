using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Utils.TargetSelection
{
    public abstract class BasicTargetSelectionLogic
    {
        public BasicTargetSelectionLogic(AmeisenBotInterfaces bot)
        {
            Bot = bot;
        }

        public IEnumerable<int> BlacklistedTargets { get; set; }

        public AmeisenBotInterfaces Bot { get; }

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
                && Bot.Db.GetReaction(wowUnit, Bot.Player) == WowUnitReaction.Hostile
                && wowUnit.DistanceTo(Bot.Player) < 80.0f;
        }
    }
}