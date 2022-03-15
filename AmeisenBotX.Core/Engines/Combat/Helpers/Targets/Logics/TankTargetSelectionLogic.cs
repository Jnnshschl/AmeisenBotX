using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Combat.Helpers.Targets.Logics
{
    public class TankTargetSelectionLogic : BasicTargetSelectionLogic
    {
        public TankTargetSelectionLogic(AmeisenBotInterfaces bot) : base(bot)
        {
        }

        public override bool SelectTarget(out IEnumerable<IWowUnit> possibleTargets)
        {
            possibleTargets = null;

            IEnumerable<IWowUnit> unitsAroundMe = Bot.Objects.WowObjects
                .OfType<IWowUnit>()
                .Where(e => IsValidEnemy(e))
                .OrderByDescending(e => e.Type)
                .ThenByDescending(e => e.MaxHealth);

            IEnumerable<IWowUnit> targetsINeedToTank = unitsAroundMe
                .Where(e => e.Type != WowObjectType.Player
                         && e.TargetGuid != Bot.Wow.PlayerGuid
                         && Bot.Objects.PartymemberGuids.Contains(e.TargetGuid));

            if (targetsINeedToTank.Any())
            {
                possibleTargets = targetsINeedToTank;
                return true;
            }
            else
            {
                if (Bot.Objects.Partymembers.Any())
                {
                    Dictionary<IWowUnit, int> targets = new();

                    foreach (IWowUnit unit in Bot.Objects.Partymembers)
                    {
                        if (unit.TargetGuid > 0)
                        {
                            IWowUnit targetUnit = Bot.GetWowObjectByGuid<IWowUnit>(unit.TargetGuid);

                            if (targetUnit != null && Bot.Db.GetReaction(targetUnit, Bot.Player) != WowUnitReaction.Friendly)
                            {
                                if (!targets.ContainsKey(targetUnit))
                                {
                                    targets.Add(targetUnit, 1);
                                }
                                else
                                {
                                    ++targets[targetUnit];
                                }
                            }
                        }
                    }

                    possibleTargets = targets.OrderBy(e => e.Value).Select(e => e.Key);
                    return true;
                }

                possibleTargets = unitsAroundMe;
                return true;
            }
        }
    }
}