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
            possibleTargets = null;

            if (Bot.Wow.TargetGuid == 0 || Bot.Target == null)
            {
                IEnumerable<IWowUnit> priorityTargets = Bot.Objects.WowObjects
                    .OfType<IWowUnit>()
                    .Where(e => IsValidUnit(e) && IsPriorityTarget(e))
                    .OrderBy(e => e.DistanceTo(Bot.Player));

                if (priorityTargets.Any())
                {
                    possibleTargets = priorityTargets;
                    return true;
                }

                possibleTargets = Bot.Objects.WowObjects
                    .OfType<IWowUnit>()
                    .Where(e => IsValidUnit(e))
                    .OrderByDescending(e => e.Type)
                    .ThenBy(e => e.DistanceTo(Bot.Player));

                return true;
            }
            else if (!IsValidUnit(Bot.Target))
            {
                Bot.Wow.ClearTarget();
                return true;
            }

            return false;
        }
    }
}