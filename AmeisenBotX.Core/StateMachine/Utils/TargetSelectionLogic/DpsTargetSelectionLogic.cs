using AmeisenBotX.Core.Data.Objects.WowObjects;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Statemachine.Utils.TargetSelectionLogic
{
    public class DpsTargetSelectionLogic : BasicTargetSelectionLogic
    {
        public override bool SelectTarget(out IEnumerable<WowUnit> possibleTargets)
        {
            possibleTargets = null;

            if (WowInterface.I.ObjectManager.TargetGuid == 0 || WowInterface.I.Target == null)
            {
                IEnumerable<WowUnit> priorityTargets = WowInterface.I.ObjectManager.WowObjects
                    .OfType<WowUnit>()
                    .Where(e => IsValidUnit(e) && IsPriorityTarget(e))
                    .OrderBy(e => e.DistanceTo(WowInterface.I.Player));

                if (priorityTargets.Any())
                {
                    possibleTargets = priorityTargets;
                    return true;
                }

                possibleTargets = WowInterface.I.ObjectManager.WowObjects
                    .OfType<WowUnit>()
                    .Where(e => IsValidUnit(e))
                    .OrderByDescending(e => e.Type)
                    .ThenBy(e => e.DistanceTo(WowInterface.I.Player));

                return true;
            }
            else if (!IsValidUnit(WowInterface.I.Target))
            {
                WowInterface.I.HookManager.WowClearTarget();
                return true;
            }

            return false;
        }
    }
}