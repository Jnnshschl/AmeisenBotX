using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Utils.TargetSelection
{
    public class TankTargetSelectionLogic : BasicTargetSelectionLogic
    {
        public TankTargetSelectionLogic(WowInterface wowInterface) : base(wowInterface)
        {
        }

        public override bool SelectTarget(out IEnumerable<WowUnit> possibleTargets)
        {
            possibleTargets = null;

            if (WowInterface.Target != null
                && WowInterface.Target.Guid != 0)
            {
                if (!IsValidUnit(WowInterface.Target))
                {
                    WowInterface.NewWowInterface.WowClearTarget();
                    return true;
                }

                if (WowInterface.Target.Type != WowObjectType.Player
                    && WowInterface.Target.TargetGuid != WowInterface.Player.Guid
                    && WowInterface.Objects.PartymemberGuids.Contains(WowInterface.Target.TargetGuid))
                {
                    return true;
                }
            }

            IEnumerable<WowUnit> unitsAroundMe = WowInterface.Objects.WowObjects
                .OfType<WowUnit>()
                .Where(e => IsValidUnit(e))
                .OrderByDescending(e => e.Type)
                .ThenByDescending(e => e.MaxHealth);

            IEnumerable<WowUnit> targetsINeedToTank = unitsAroundMe
                .Where(e => e.Type != WowObjectType.Player
                         && e.TargetGuid != WowInterface.Player.Guid
                         && WowInterface.Objects.PartymemberGuids.Contains(e.TargetGuid));

            if (targetsINeedToTank.Any())
            {
                possibleTargets = targetsINeedToTank;
                return true;
            }
            else
            {
                if (WowInterface.Objects.Partymembers.Any())
                {
                    Dictionary<WowUnit, int> targets = new();

                    foreach (WowUnit unit in WowInterface.Objects.Partymembers)
                    {
                        if (unit.TargetGuid > 0)
                        {
                            WowUnit targetUnit = WowInterface.Objects.GetWowObjectByGuid<WowUnit>(unit.TargetGuid);

                            if (targetUnit != null && WowInterface.Db.GetReaction(targetUnit, WowInterface.Player) != WowUnitReaction.Friendly)
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