using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Objects.WowObjects;
using AmeisenBotX.Core.Statemachine.Utils.TargetSelectionLogic;
using System;
using System.Collections.Generic;

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

        public IEnumerable<string> PriorityTargets { get; set; }

        public ITargetSelectionLogic TargetSelectionLogic { get; }

        private TimegatedEvent TargetSwitchEvent { get; set; }

        public bool GetUnitToTarget(out IEnumerable<WowUnit> possibleTargets)
        {
            TargetSelectionLogic.PriorityTargets = PriorityTargets;

            if (TargetSwitchEvent.Run() && TargetSelectionLogic.SelectTarget(out IEnumerable<WowUnit> possibleTargetsFromLogic))
            {
                possibleTargets = possibleTargetsFromLogic;
                return true;
            }

            possibleTargets = null;
            return false;
        }
    }
}