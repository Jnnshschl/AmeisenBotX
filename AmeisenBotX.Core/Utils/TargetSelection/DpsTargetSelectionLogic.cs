using AmeisenBotX.Core.Data.Objects;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Utils.TargetSelection
{
    public class DpsTargetSelectionLogic : BasicTargetSelectionLogic
    {
        public DpsTargetSelectionLogic(WowInterface wowInterface) : base(wowInterface)
        {
        }

        public override bool SelectTarget(out IEnumerable<WowUnit> possibleTargets)
        {
            possibleTargets = null;

            if (WowInterface.TargetGuid == 0 || WowInterface.Target == null)
            {
                IEnumerable<WowUnit> priorityTargets = WowInterface.ObjectManager.WowObjects
                    .OfType<WowUnit>()
                    .Where(e => IsValidUnit(e) && IsPriorityTarget(e))
                    .OrderBy(e => e.DistanceTo(WowInterface.Player));

                if (priorityTargets.Any())
                {
                    possibleTargets = priorityTargets;
                    return true;
                }

                possibleTargets = WowInterface.ObjectManager.WowObjects
                    .OfType<WowUnit>()
                    .Where(e => IsValidUnit(e))
                    .OrderByDescending(e => e.Type)
                    .ThenBy(e => e.DistanceTo(WowInterface.Player));

                return true;
            }
            else if (!IsValidUnit(WowInterface.Target))
            {
                WowInterface.HookManager.WowClearTarget();
                return true;
            }

            return false;
        }
    }
}