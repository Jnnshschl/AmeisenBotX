using AmeisenBotX.Core.Engines.Combat.Helpers.Targets.Priority.Basic;
using AmeisenBotX.Core.Engines.Combat.Helpers.Targets.Priority.Special;
using AmeisenBotX.Core.Engines.Combat.Helpers.Targets.Validation.Basic;
using AmeisenBotX.Core.Engines.Combat.Helpers.Targets.Validation.Special;
using AmeisenBotX.Core.Engines.Combat.Helpers.Targets.Validation.Util;
using AmeisenBotX.Wow.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Combat.Helpers.Targets.Logics.Dps
{
    public class SimpleDpsTargetSelectionLogic : BasicTargetSelectionLogic
    {
        public SimpleDpsTargetSelectionLogic(AmeisenBotInterfaces bot) : base(bot)
        {
            TargetValidator.Add(new IsAttackableTargetValidator(bot));
            TargetValidator.Add(new IsInCombatTargetValidator());
            TargetValidator.Add(new IsThreatTargetValidator(bot));
            TargetValidator.Add(new DungeonTargetValidator(bot));
            TargetValidator.Add(new CachedTargetValidator(new IsReachableTargetValidator(bot), TimeSpan.FromSeconds(4)));

            TargetPrioritizer.Add(new ListTargetPrioritizer());
            TargetPrioritizer.Add(new DungeonTargetPrioritizer(bot));
        }

        public override bool SelectTarget(out IEnumerable<IWowUnit> possibleTargets)
        {
            possibleTargets = Bot.Objects.All.OfType<IWowUnit>()
                .Where(e => TargetValidator.IsValid(e))
                .OrderByDescending(e => IsPriorityTarget(e))
                .ThenByDescending(e => e.Type)
                .ThenBy(e => e.DistanceTo(Bot.Player));

            return possibleTargets != null && possibleTargets.Any();
        }
    }
}