using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Objects.WowObject;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Statemachine.Utils.TargetSelectionLogic
{
    public class TankTargetSelectionLogic : ITargetSelectionLogic
    {
        public TankTargetSelectionLogic(WowInterface wowInterface)
        {
            WowInterface = wowInterface;
        }

        private WowInterface WowInterface { get; }

        public void Reset()
        {
        }

        public bool SelectTarget(out List<WowUnit> possibleTargets)
        {
            if (WowInterface.ObjectManager.Target != null && WowInterface.ObjectManager.TargetGuid != 0 && (WowInterface.ObjectManager.Target.IsDead || !BotUtils.IsValidUnit(WowInterface.ObjectManager.Target)))
            {
                WowInterface.HookManager.ClearTarget();
            }

            bool insertCurrentTargetToTop = WowInterface.ObjectManager.Target != null && WowInterface.ObjectManager.TargetGuid != 0
                && !WowInterface.ObjectManager.Target.IsDead && BotUtils.IsValidUnit(WowInterface.ObjectManager.Target)
                && WowInterface.ObjectManager.Target.TargetGuid != WowInterface.ObjectManager.PlayerGuid
                && WowInterface.HookManager.GetUnitReaction(WowInterface.ObjectManager.Target, WowInterface.ObjectManager.Target) != WowUnitReaction.Friendly;

            // get all enemies targeting our group
            List<WowUnit> enemies = WowInterface.ObjectManager.GetNearEnemies<WowUnit>(WowInterface.ObjectManager.Player.Position, 100)
                .Where(e => e.TargetGuid != 0 && !e.IsDead && e.IsInCombat && WowInterface.ObjectManager.PartymemberGuids.Contains(e.TargetGuid)).ToList();

            if (enemies.Count > 0)
            {
                // filter out enemies already attacking me
                List<WowUnit> enemiesNotTargetingMe = enemies
                    .Where(e => e.TargetGuid != WowInterface.ObjectManager.PlayerGuid).ToList();

                if (enemiesNotTargetingMe.Count > 0)
                {
                    List<WowUnit> targetUnits = enemiesNotTargetingMe
                        .Where(e => WowInterface.ObjectManager.TargetGuid != e.Guid)
                        .OrderBy(e => e.Position.GetDistance(WowInterface.ObjectManager.Player.Position)).ToList();

                    if (targetUnits != null && targetUnits.Count > 0)
                    {
                        if (insertCurrentTargetToTop)
                        {
                            targetUnits.Insert(0, WowInterface.ObjectManager.Target);
                        }

                        // target closest enemy
                        possibleTargets = targetUnits;
                        return true;
                    }
                }
                else
                {
                    // target the unit with the most health, likely to be the boss
                    List<WowUnit> targetUnits = enemies
                        .Where(e => WowInterface.ObjectManager.TargetGuid != e.Guid)
                        .OrderBy(e => e.Health).ToList();

                    if (targetUnits != null && targetUnits.Count > 0)
                    {
                        if (insertCurrentTargetToTop)
                        {
                            targetUnits.Insert(0, WowInterface.ObjectManager.Target);
                        }

                        // target closest enemy
                        possibleTargets = targetUnits;
                        return true;
                    }
                }
            }

            possibleTargets = null;
            return false;
        }
    }
}