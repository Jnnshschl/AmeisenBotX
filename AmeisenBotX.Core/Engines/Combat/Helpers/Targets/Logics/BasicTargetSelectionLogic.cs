using AmeisenBotX.Core.Engines.Combat.Helpers.Targets.Priority;
using AmeisenBotX.Core.Engines.Combat.Helpers.Targets.Validation;
using AmeisenBotX.Core.Engines.Combat.Helpers.Targets.Validation.Basic;
using AmeisenBotX.Wow.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Combat.Helpers.Targets.Logics
{
    public abstract class BasicTargetSelectionLogic(AmeisenBotInterfaces bot)
    {
        public IEnumerable<int> BlacklistedTargets { get; set; }

        public AmeisenBotInterfaces Bot { get; } = bot;

        public IEnumerable<int> PriorityTargets { get; set; }

        public TargetPriorityManager TargetPrioritizer { get; set; } = new();

        public TargetValidationManager TargetValidator { get; set; } = new(new IsValidAliveTargetValidator());

        public abstract bool SelectTarget(out IEnumerable<IWowUnit> wowUnit);

        protected bool IsPriorityTarget(IWowUnit wowUnit)
        {
            return PriorityTargets != null && PriorityTargets.Contains(wowUnit.DisplayId);
        }
    }
}