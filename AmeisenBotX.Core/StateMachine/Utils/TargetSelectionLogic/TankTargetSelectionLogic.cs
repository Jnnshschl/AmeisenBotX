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

        public List<string> PriorityTargets { get; set; }

        public void Reset()
        {
        }

        public bool SelectTarget(out List<WowUnit> possibleTargets)
        {
            if (WowInterface.ObjectManager.Target != null
                && WowInterface.ObjectManager.TargetGuid != 0
                && (WowInterface.ObjectManager.Target.IsDead
                || !BotUtils.IsValidUnit(WowInterface.ObjectManager.Target)))
            {
                WowInterface.HookManager.ClearTarget();
            }

            bool insertCurrentTargetToTop = WowInterface.ObjectManager.Target != null
                && WowInterface.ObjectManager.TargetGuid != 0
                && !WowInterface.ObjectManager.Target.IsDead && BotUtils.IsValidUnit(WowInterface.ObjectManager.Target)
                && WowInterface.ObjectManager.Target.TargetGuid != WowInterface.ObjectManager.PlayerGuid
                && WowInterface.HookManager.GetUnitReaction(WowInterface.ObjectManager.Player, WowInterface.ObjectManager.Target) != WowUnitReaction.Friendly;

            if (insertCurrentTargetToTop)
            {
                possibleTargets = new List<WowUnit>() { WowInterface.ObjectManager.Target };
                return true;
            }

            // get all enemies targeting our group
            IEnumerable<WowUnit> enemies = WowInterface.ObjectManager
                .GetEnemiesTargetingPartymembers(WowInterface.ObjectManager.Player.Position, 100);

            if (enemies.Count() > 0)
            {
                // filter out enemies already attacking me
                IEnumerable<WowUnit> enemiesNotTargetingMe = enemies
                    .Where(e => e.TargetGuid != WowInterface.ObjectManager.PlayerGuid);

                if (enemiesNotTargetingMe.Count() > 0)
                {
                    IEnumerable<WowUnit> targetUnits = enemiesNotTargetingMe
                        .Where(e => WowInterface.ObjectManager.TargetGuid != e.Guid)
                        .OrderBy(e => e.Position.GetDistance(WowInterface.ObjectManager.Player.Position));

                    if (targetUnits != null && targetUnits.Count() > 0)
                    {
                        // target closest enemy
                        possibleTargets = targetUnits.ToList();
                        return true;
                    }
                }
                else
                {
                    // target the unit with the most health, likely to be the boss
                    IEnumerable<WowUnit> targetUnits = enemies
                        .Where(e => WowInterface.ObjectManager.TargetGuid != e.Guid)
                        .OrderBy(e => e.Health);

                    if (targetUnits != null && targetUnits.Count() > 0)
                    {
                        // target closest enemy
                        possibleTargets = targetUnits.ToList();
                        return true;
                    }
                }

                possibleTargets = enemies.Where(e => e.IsTaggedByMe || !e.IsTaggedByOther).ToList();
                return true;
            }

            possibleTargets = null;
            return false;
        }
    }
}