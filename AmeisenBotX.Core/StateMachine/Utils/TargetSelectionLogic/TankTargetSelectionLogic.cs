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

        public bool SelectTarget(out List<WowUnit> targetToSelect)
        {
            if (WowInterface.ObjectManager.Target != null)
            {
                if (!WowInterface.ObjectManager.Target.IsDead && WowInterface.ObjectManager.Target.TargetGuid != WowInterface.ObjectManager.PlayerGuid)
                {
                    targetToSelect = null;
                    return false;
                }
            }

            // get all enemies targeting our group
            List<WowUnit> enemies = WowInterface.ObjectManager.GetNearEnemies<WowUnit>(WowInterface.ObjectManager.Player.Position, 100)
                .Where(e => e.TargetGuid != 0 && WowInterface.ObjectManager.PartymemberGuids.Contains(e.TargetGuid)).ToList();

            if (enemies.Count > 0)
            {
                // filter out enemies already attacking me
                List<WowUnit> enemiesNotTargetingMe = enemies
                    .Where(e => e.TargetGuid != WowInterface.ObjectManager.PlayerGuid).ToList();

                if (enemiesNotTargetingMe.Count > 0)
                {
                    WowUnit targetUnit = enemiesNotTargetingMe.OrderBy(e => e.Position.GetDistance(WowInterface.ObjectManager.Player.Position)).FirstOrDefault();

                    if (targetUnit != null && targetUnit.Guid > 0 && WowInterface.ObjectManager.TargetGuid != targetUnit.Guid)
                    {
                        // target closest enemy
                        targetToSelect = new List<WowUnit>() { targetUnit };
                        return true;
                    }
                }
                else
                {
                    WowUnit targetUnit = enemies.OrderBy(e => e.Position.GetDistance(WowInterface.ObjectManager.Player.Position)).FirstOrDefault();

                    if (targetUnit != null && targetUnit.Guid > 0 && WowInterface.ObjectManager.TargetGuid != targetUnit.Guid)
                    {
                        // target closest enemy
                        targetToSelect = new List<WowUnit>() { targetUnit };
                        return true;
                    }
                }
            }

            targetToSelect = null;
            return false;
        }
    }
}