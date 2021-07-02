using AmeisenBotX.Core.Data.Objects;
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

        public override bool SelectTarget(out IEnumerable<WowUnit> possibleTargets)
        {
            possibleTargets = null;

            if (Bot.Target != null
                && Bot.Wow.TargetGuid != 0)
            {
                if (!IsValidUnit(Bot.Target))
                {
                    Bot.Wow.WowClearTarget();
                    return true;
                }

                if (Bot.Target.Type != WowObjectType.Player
                    && Bot.Target.TargetGuid != Bot.Wow.PlayerGuid
                    && Bot.Objects.PartymemberGuids.Contains(Bot.Target.TargetGuid))
                {
                    return true;
                }
            }

            IEnumerable<WowUnit> unitsAroundMe = Bot.Objects.WowObjects
                .OfType<WowUnit>()
                .Where(e => IsValidUnit(e))
                .OrderByDescending(e => e.Type)
                .ThenByDescending(e => e.MaxHealth);

            IEnumerable<WowUnit> targetsINeedToTank = unitsAroundMe
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
                    Dictionary<WowUnit, int> targets = new();

                    foreach (WowUnit unit in Bot.Objects.Partymembers)
                    {
                        if (unit.TargetGuid > 0)
                        {
                            WowUnit targetUnit = Bot.GetWowObjectByGuid<WowUnit>(unit.TargetGuid);

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