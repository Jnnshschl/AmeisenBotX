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
            MinTargetSwitchTime = minTargetSwitchTime;

            PriorityTargets = new List<string>();
        }

        public List<string> PriorityTargets { get; set; }

        public ITargetSelectionLogic TargetSelectionLogic { get; }

        private DateTime LastTargetSwitch { get; set; }

        private TimeSpan MinTargetSwitchTime { get; }

        public bool GetUnitToTarget(out List<WowUnit> possibleTargets)
        {
            bool result = TargetSelectionLogic != null                      // we cant use the logic if its null
                && DateTime.Now - LastTargetSwitch > MinTargetSwitchTime;   // limit the target switches by time

            if (result && TargetSelectionLogic.SelectTarget(out List<WowUnit> possibleTargetsFromLogic))
            {
                // if everything went well, we reset the time gate
                LastTargetSwitch = DateTime.Now;

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