using AmeisenBotX.Core.Data.Objects;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Combat.Helpers.Targets.Logics
{
    public class DpsTargetSelectionLogic : BasicTargetSelectionLogic
    {
        public DpsTargetSelectionLogic(AmeisenBotInterfaces bot) : base(bot)
        {
        }

        public override bool SelectTarget(out IEnumerable<WowUnit> possibleTargets)
        {
            possibleTargets = null;

            if (Bot.Wow.TargetGuid == 0 || Bot.Target == null)
            {
                IEnumerable<WowUnit> priorityTargets = Bot.Objects.WowObjects
                    .OfType<WowUnit>()
                    .Where(e => IsValidUnit(e) && IsPriorityTarget(e))
                    .OrderBy(e => e.DistanceTo(Bot.Player));

                if (priorityTargets.Any())
                {
                    possibleTargets = priorityTargets;
                    return true;
                }

                possibleTargets = Bot.Objects.WowObjects
                    .OfType<WowUnit>()
                    .Where(e => IsValidUnit(e))
                    .OrderByDescending(e => e.Type)
                    .ThenBy(e => e.DistanceTo(Bot.Player));

                return true;
            }
            else if (!IsValidUnit(Bot.Target))
            {
                Bot.Wow.WowClearTarget();
                return true;
            }

            return false;
        }
    }
}