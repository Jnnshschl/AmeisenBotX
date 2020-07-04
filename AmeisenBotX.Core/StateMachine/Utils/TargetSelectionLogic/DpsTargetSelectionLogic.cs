using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Objects.WowObject;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Statemachine.Utils.TargetSelectionLogic
{
    public class DpsTargetSelectionLogic : ITargetSelectionLogic
    {
        public DpsTargetSelectionLogic(WowInterface wowInterface)
        {
            WowInterface = wowInterface;
        }

        private WowInterface WowInterface { get; }

        public void Reset()
        {
        }

        public bool SelectTarget(out List<WowUnit> targetToSelect)
        {
            if (WowInterface.ObjectManager.TargetGuid != 0
                && (WowInterface.ObjectManager.Target.IsDead
                    || WowInterface.ObjectManager.Target.IsNotAttackable
                    || !BotUtils.IsValidUnit(WowInterface.ObjectManager.Target)
                    || WowInterface.HookManager.GetUnitReaction(WowInterface.ObjectManager.Player, WowInterface.ObjectManager.Target) == WowUnitReaction.Friendly))
            {
                WowInterface.HookManager.ClearTarget();
            }

            IEnumerable<WowUnit> nearEnemies = WowInterface.ObjectManager
                .GetNearEnemies<WowUnit>(WowInterface.ObjectManager.Player.Position, 100)
                .Where(e => BotUtils.IsValidUnit(e) && !e.IsDead)
                .OrderBy(e => e.Position.GetDistance(WowInterface.ObjectManager.Player.Position));

            // get enemies targeting my partymembers
            IEnumerable<WowUnit> enemies = nearEnemies
                .Where(e => e.TargetGuid != 0 && WowInterface.ObjectManager.PartymemberGuids.Contains(e.TargetGuid));

            // TODO: need to handle duels, our target will
            // be friendly there but is attackable
            if (enemies.Count() > 0)
            {
                targetToSelect = new List<WowUnit>() { enemies.FirstOrDefault() };

                if (targetToSelect != null)
                {
                    return true;
                }
            }

            // get enemies tagged by me or no one, or players
            enemies = nearEnemies
                .Where(e => e.IsTaggedByMe || !e.IsTaggedByOther || e.GetType() == typeof(WowPlayer));

            if (enemies.Count() > 0)
            {
                targetToSelect = enemies.ToList();
                return true;
            }

            targetToSelect = null;
            return false;
        }
    }
}