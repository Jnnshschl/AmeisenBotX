using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Engines.Combat.Helpers.Targets.Logics;
using AmeisenBotX.Core.Engines.Combat.Helpers.Targets.Logics.Dps;
using AmeisenBotX.Core.Engines.Combat.Helpers.Targets.Logics.Heal;
using AmeisenBotX.Core.Engines.Combat.Helpers.Targets.Logics.Tank;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Combat.Helpers.Targets
{
    public class TargetManager : ITargetProvider
    {
        public TargetManager(AmeisenBotInterfaces bot, WowRole role, TimeSpan minTargetSwitchTime)
        {
            TargetSelectionLogic = role switch
            {
                WowRole.Dps => new SimpleDpsTargetSelectionLogic(bot),
                WowRole.Tank => new SimpleTankTargetSelectionLogic(bot),
                WowRole.Heal => new SimpleHealTargetSelectionLogic(bot),
                _ => throw new NotImplementedException(),
            };

            TargetSwitchEvent = new(minTargetSwitchTime);
        }

        public TargetManager(BasicTargetSelectionLogic targetSelectionLogic, TimeSpan minTargetSwitchTime)
        {
            TargetSelectionLogic = targetSelectionLogic;
            TargetSwitchEvent = new(minTargetSwitchTime);
        }

        public IEnumerable<int> BlacklistedTargets
        {
            get => TargetSelectionLogic.TargetValidator.BlacklistTargetValidator.Blacklist;
            set => TargetSelectionLogic.TargetValidator.BlacklistTargetValidator.Blacklist = value;
        }

        public IEnumerable<int> PriorityTargets
        {
            get => TargetSelectionLogic.TargetPrioritizer.ListTargetPrioritizer.PriorityDisplayIds;
            set => TargetSelectionLogic.TargetPrioritizer.ListTargetPrioritizer.PriorityDisplayIds = value;
        }

        private IEnumerable<IWowUnit> PossibleTargets { get; set; }

        private BasicTargetSelectionLogic TargetSelectionLogic { get; }

        private TimegatedEvent TargetSwitchEvent { get; set; }

        public bool Get(out IEnumerable<IWowUnit> possibleTargets)
        {
            if (TargetSwitchEvent.Run())
            {
                PossibleTargets = TargetSelectionLogic.SelectTarget(out IEnumerable<IWowUnit> newPossibleTargets) ? newPossibleTargets : null;
            }

            possibleTargets = PossibleTargets;
            return PossibleTargets != null && PossibleTargets.Any();
        }
    }
}