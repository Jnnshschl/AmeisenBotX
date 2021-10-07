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
                possibleTargets = Bot.Objects.WowObjects.OfType<IWowUnit>()
                    .Where(e => IsValidUnit(e))
                    .OrderByDescending(e => e.Type)
                    .ThenBy(e => e.DistanceTo(Bot.Player));

                IEnumerable<IWowUnit> priorityTargets = possibleTargets
                    .Where(e => IsPriorityTarget(e));

                if (priorityTargets.Any())
                {
                    possibleTargets = priorityTargets;
                    return true;
                }

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