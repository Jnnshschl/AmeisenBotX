using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Statemachine.Utils.TargetSelectionLogic;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Statemachine.Utils
{
    public class TargetManager
    {
        public TargetManager(ITargetSelectionLogic targetSelectionLogic, TimeSpan minTargetSwitchTime)
        {
            TargetSelectionLogic = targetSelectionLogic;

            TargetSwitchEvent = new TimegatedEvent(minTargetSwitchTime);

            PriorityTargets = new List<string>();
        }

        public List<string> PriorityTargets { get; set; }

        public ITargetSelectionLogic TargetSelectionLogic { get; }

        private TimegatedEvent TargetSwitchEvent { get; set; }

        public bool GetUnitToTarget(out List<WowUnit> possibleTargets)
        {
            if (TargetSwitchEvent.Run() && TargetSelectionLogic.SelectTarget(out List<WowUnit> possibleTargetsFromLogic))
            {
                // move the priority unit to the start of the list
                if (PriorityTargets != null && PriorityTargets.Count > 0)
                {
                    WowUnit priorityUnit = possibleTargetsFromLogic.FirstOrDefault(e => PriorityTargets.Any(x => x.Equals(e.Name, StringComparison.OrdinalIgnoreCase)));
                    if (priorityUnit != null)
                    {
                        int index = possibleTargetsFromLogic.IndexOf(priorityUnit);
                        possibleTargetsFromLogic.RemoveAt(index);
                        possibleTargetsFromLogic.Insert(0, priorityUnit);
                    }
                }

                possibleTargets = possibleTargetsFromLogic;
                return true;
            }

            possibleTargets = null;
            return false;
        }
    }
}