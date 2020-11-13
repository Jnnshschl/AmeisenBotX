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

        public IEnumerable<string> PriorityTargets { get => TargetSelectionLogic.PriorityTargets; set => TargetSelectionLogic.PriorityTargets = value; }

        public ITargetSelectionLogic TargetSelectionLogic { get; }

        public IEnumerable<string> BlacklistedTargets { get => TargetSelectionLogic.BlacklistedTargets; set => TargetSelectionLogic.BlacklistedTargets = value; }

        private TimegatedEvent TargetSwitchEvent { get; set; }

        public bool GetUnitToTarget(out IEnumerable<WowUnit> possibleTargets)
        {
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