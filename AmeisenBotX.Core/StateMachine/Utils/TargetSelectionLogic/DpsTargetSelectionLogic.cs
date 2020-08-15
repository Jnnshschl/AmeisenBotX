using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
using System;
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

        public IEnumerable<string> PriorityTargets { get; set; }

        private WowInterface WowInterface { get; }

        public void Reset()
        {
        }

        public bool SelectTarget(out IEnumerable<WowUnit> possibleTargets)
        {
            if (WowInterface.ObjectManager.TargetGuid != 0
                && (WowInterface.ObjectManager.Target.IsDead
                    || WowInterface.ObjectManager.Target.IsNotAttackable
                    || !BotUtils.IsValidUnit(WowInterface.ObjectManager.Target)
                    || WowInterface.HookManager.GetUnitReaction(WowInterface.ObjectManager.Player, WowInterface.ObjectManager.Target) == WowUnitReaction.Friendly))
            {
                WowInterface.HookManager.ClearTarget();
            }

            if (PriorityTargets != null && PriorityTargets.Any())
            {
                IEnumerable<WowUnit> nearPriorityEnemies = WowInterface.ObjectManager.WowObjects.OfType<WowUnit>()
                    .Where(e => BotUtils.IsValidUnit(e) && !e.IsDead && PriorityTargets.Any(x => e.Name.Equals(x, StringComparison.OrdinalIgnoreCase)))
                    .OrderBy(e => e.Position.GetDistance(WowInterface.ObjectManager.Player.Position));

                if (nearPriorityEnemies != null && nearPriorityEnemies.Any())
                {
                    possibleTargets = nearPriorityEnemies.ToList();
                    return true;
                }
            }

            IEnumerable<WowUnit> nearEnemies = WowInterface.ObjectManager
                .GetNearEnemies<WowUnit>(WowInterface.ObjectManager.Player.Position, 100.0)
                .Where(e => BotUtils.IsValidUnit(e)
                    && !e.IsDead
                    && e.IsInCombat
                    && !(WowInterface.ObjectManager.MapId == MapId.HallsOfReflection && e.Name == "The Lich King")
                    && !(WowInterface.ObjectManager.MapId == MapId.DrakTharonKeep && WowInterface.ObjectManager.WowObjects.OfType<WowDynobject>().Any(e => e.SpellId == 47346))) // Novos fix
                .OrderBy(e => e.GetType().Name) // make sure players are at the top (pvp)
                .ThenByDescending(e => e.IsFleeing) // catch fleeing enemies
                .ThenBy(e => e.Position.GetDistance(WowInterface.ObjectManager.Player.Position));

            // TODO: need to handle duels, our target will
            // be friendly there but is attackable
            if (nearEnemies.Any())
            {
                possibleTargets = new List<WowUnit>() { nearEnemies.FirstOrDefault() };

                if (possibleTargets != null)
                {
                    return true;
                }
            }

            // get enemies tagged by me or no one, or players
            nearEnemies = nearEnemies.Where(e => e.IsTaggedByMe || !e.IsTaggedByOther || e.GetType() == typeof(WowPlayer));

            if (nearEnemies.Any())
            {
                possibleTargets = nearEnemies;
                return true;
            }

            possibleTargets = null;
            return false;
        }
    }
}