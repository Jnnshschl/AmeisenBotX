﻿using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Utils.TargetSelection;
using System;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Utils
{
    public class TargetManager : ITargetProvider
    {
        public TargetManager(BasicTargetSelectionLogic targetSelectionLogic, TimeSpan minTargetSwitchTime)
        {
            TargetSelectionLogic = targetSelectionLogic;
            TargetSwitchEvent = new(minTargetSwitchTime);
            PriorityTargets = new List<int>();
        }

        public IEnumerable<int> BlacklistedTargets { get => TargetSelectionLogic.BlacklistedTargets; set => TargetSelectionLogic.BlacklistedTargets = value; }

        public IEnumerable<int> PriorityTargets { get => TargetSelectionLogic.PriorityTargets; set => TargetSelectionLogic.PriorityTargets = value; }

        private BasicTargetSelectionLogic TargetSelectionLogic { get; }

        private TimegatedEvent TargetSwitchEvent { get; set; }

        public bool Get(out IEnumerable<WowUnit> possibleTargets)
        {
            if (TargetSwitchEvent.Run() && TargetSelectionLogic.SelectTarget(out possibleTargets))
            {
                return true;
            }
            else
            {
                possibleTargets = null;
                return false;
            }
        }
    }
}