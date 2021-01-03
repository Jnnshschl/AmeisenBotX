using AmeisenBotX.Core.Data.Objects.WowObjects;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Statemachine.Utils.TargetSelectionLogic
{
    public class TankTargetSelectionLogic : BasicTargetSelectionLogic
    {
        public override bool SelectTarget(out IEnumerable<WowUnit> possibleTargets)
        {
            possibleTargets = null;

            bool hasTarget = WowInterface.I.Target != null
                && WowInterface.I.ObjectManager.TargetGuid != 0;

            if (hasTarget)
            {
                if (!IsValidUnit(WowInterface.I.Target))
                {
                    WowInterface.I.HookManager.WowClearTarget();
                    return true;
                }

                if (WowInterface.I.Target.Type != WowObjectType.Player
                    && WowInterface.I.Target.TargetGuid != WowInterface.I.ObjectManager.PlayerGuid
                    && WowInterface.I.ObjectManager.PartymemberGuids.Contains(WowInterface.I.Target.TargetGuid))
                {
                    return true;
                }
            }

            IEnumerable<WowUnit> unitsAroundMe = WowInterface.I.ObjectManager.WowObjects
                .OfType<WowUnit>()
                .Where(e => IsValidUnit(e))
                .OrderByDescending(e => e.Type)
                .ThenByDescending(e => e.MaxHealth);

            IEnumerable<WowUnit> targetsINeedToTank = unitsAroundMe
                .Where(e => e.Type != WowObjectType.Player
                         && e.TargetGuid != WowInterface.I.Player.Guid
                         && WowInterface.I.ObjectManager.PartymemberGuids.Contains(e.TargetGuid));

            if (targetsINeedToTank.Any())
            {
                possibleTargets = targetsINeedToTank;
                return true;
            }
            else
            {
                if (WowInterface.I.ObjectManager.Partymembers.Any())
                {
                    Dictionary<WowUnit, int> targets = new Dictionary<WowUnit, int>();

                    foreach (WowUnit unit in WowInterface.I.ObjectManager.Partymembers)
                    {
                        if (unit.TargetGuid > 0)
                        {
                            WowUnit targetUnit = WowInterface.I.ObjectManager.GetWowObjectByGuid<WowUnit>(unit.TargetGuid);

                            if (targetUnit != null && WowInterface.I.HookManager.WowGetUnitReaction(targetUnit, WowInterface.I.Player) != WowUnitReaction.Friendly)
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