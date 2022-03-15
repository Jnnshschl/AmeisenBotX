using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Engines.Combat.Helpers.Targets.Logics;
using AmeisenBotX.Wow.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Combat.Helpers.Targets
{
    public class TargetManager : ITargetProvider
    {
        public TargetManager(BasicTargetSelectionLogic targetSelectionLogic, TimeSpan minTargetSwitchTime)
        {
            TargetSelectionLogic = targetSelectionLogic;
            TargetSwitchEvent = new(minTargetSwitchTime);
        }

        public IEnumerable<int> BlacklistedTargets { get => TargetSelectionLogic.BlacklistedTargets; set => TargetSelectionLogic.BlacklistedTargets = value; }

        public IEnumerable<int> PriorityTargets { get => TargetSelectionLogic.PriorityTargets; set => TargetSelectionLogic.PriorityTargets = value; }

        private BasicTargetSelectionLogic TargetSelectionLogic { get; }

        private TimegatedEvent TargetSwitchEvent { get; set; }

        private IEnumerable<IWowUnit> PossibleTargets { get; set; }

        public bool Get(out IEnumerable<IWowUnit> possibleTargets)
        {
            if (TargetSwitchEvent.Run())
            {
                if (TargetSelectionLogic.SelectTarget(out IEnumerable<IWowUnit> newPossibleTargets))
                {
                    PossibleTargets = newPossibleTargets;
                }
                else
                {
                    PossibleTargets = null;
                }
            }

            possibleTargets = PossibleTargets;
            return PossibleTargets != null && PossibleTargets.Any();
        }
    }
}