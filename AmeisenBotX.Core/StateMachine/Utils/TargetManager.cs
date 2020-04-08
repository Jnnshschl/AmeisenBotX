using AmeisenBotX.Core.Data.Objects.WowObject;
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
            MinTargetSwitchTime = minTargetSwitchTime;
        }

        public ITargetSelectionLogic TargetSelectionLogic { get; }

        private DateTime LastTargetSwitch { get; set; }

        private TimeSpan MinTargetSwitchTime { get; }

        public bool GetUnitToTarget(out List<WowUnit> targetToSelect)
        {
            targetToSelect = null;
            bool result = TargetSelectionLogic != null && DateTime.Now - LastTargetSwitch > MinTargetSwitchTime && TargetSelectionLogic.SelectTarget(out targetToSelect);

            if (result)
            {
                LastTargetSwitch = DateTime.Now;
            }

            return result;
        }
    }
}