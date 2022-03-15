using AmeisenBotX.Wow.Objects;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Combat.Helpers.Targets.Logics
{
    public class DpsTargetSelectionLogic : BasicTargetSelectionLogic
    {
        public DpsTargetSelectionLogic(AmeisenBotInterfaces bot) : base(bot)
        {
        }

        public override bool SelectTarget(out IEnumerable<IWowUnit> possibleTargets)
        {
            possibleTargets = Bot.Objects.WowObjects.OfType<IWowUnit>()
                .Where(e => IsValidEnemy(e))
                .OrderByDescending(e => IsPriorityTarget(e))
                .ThenByDescending(e => e.Type)
                .ThenBy(e => e.DistanceTo(Bot.Player));

            return possibleTargets != null && possibleTargets.Any();
        }
    }
}