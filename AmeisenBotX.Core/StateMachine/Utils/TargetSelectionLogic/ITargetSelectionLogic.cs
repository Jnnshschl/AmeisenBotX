using AmeisenBotX.Core.Data.Objects.WowObjects;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Statemachine.Utils.TargetSelectionLogic
{
    public interface ITargetSelectionLogic
    {
        IEnumerable<string> BlacklistedTargets { get; set; }

        IEnumerable<string> PriorityTargets { get; set; }

        void Reset();

        bool SelectTarget(out IEnumerable<WowUnit> wowUnit);
    }
}