using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObjects;
using AmeisenBotX.Logging;
using Newtonsoft.Json;
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
            if ((PriorityTargets == null || !PriorityTargets.Any()) && WowInterface.ObjectManager.MapId == MapId.UtgardeKeep)
            {
                PriorityTargets = new List<string>() { "Frost Tomb" };
            }

            if (WowInterface.ObjectManager.TargetGuid != 0 && WowInterface.ObjectManager.Target != null
                && (WowInterface.ObjectManager.Target.IsDead
                    || WowInterface.ObjectManager.Target.IsNotAttackable
                    || !BotUtils.IsValidUnit(WowInterface.ObjectManager.Target)
                    || WowInterface.HookManager.GetUnitReaction(WowInterface.ObjectManager.Player, WowInterface.ObjectManager.Target) == WowUnitReaction.Friendly))
            {
                AmeisenLogger.I.Log("TARGET", $"C | Clearing Target");
                WowInterface.HookManager.ClearTarget();
                possibleTargets = null;
                return false;
            }

            if (PriorityTargets != null && PriorityTargets.Any())
            {
                IEnumerable<WowUnit> nearPriorityEnemies = WowInterface.ObjectManager.WowObjects.OfType<WowUnit>()
                    .Where(e => BotUtils.IsValidUnit(e) && !e.IsDead && e.Health > 0 && PriorityTargets.Any(x => e.Name.Equals(x, StringComparison.OrdinalIgnoreCase)))
                    .OrderBy(e => e.Position.GetDistance(WowInterface.ObjectManager.Player.Position));

                if (nearPriorityEnemies.Any())
                {
                    AmeisenLogger.I.Log("TARGET", $"P | Targetable Units: {JsonConvert.SerializeObject(nearPriorityEnemies.Select(e => e.Name).ToList())}");
                    possibleTargets = nearPriorityEnemies;
                    return true;
                }
            }

            IEnumerable<WowUnit> nearEnemies = WowInterface.ObjectManager
                .GetEnemiesInCombatWithUs<WowUnit>(WowInterface.ObjectManager.Player.Position, 100.0)
                .Where(e => !(WowInterface.ObjectManager.MapId == MapId.HallsOfReflection && e.Name == "The Lich King")
                         && !(WowInterface.ObjectManager.MapId == MapId.DrakTharonKeep && WowInterface.ObjectManager.WowObjects.OfType<WowDynobject>().Any(e => e.SpellId == 47346))) // Novos fix
                .OrderByDescending(e => e.Type) // make sure players are at the top (pvp)
                .ThenByDescending(e => e.IsFleeing) // catch fleeing enemies
                .ThenBy(e => e.Position.GetDistance(WowInterface.ObjectManager.Player.Position));

            // TODO: need to handle duels, our target will
            // be friendly there but is attackable
            if (nearEnemies.Any())
            {
                AmeisenLogger.I.Log("TARGET", $"1 | Targetable Units: {JsonConvert.SerializeObject(nearEnemies.Select(e=>e.Name).ToList())}");
                possibleTargets = nearEnemies;
                return true;
            }

            nearEnemies = WowInterface.ObjectManager.GetNearEnemies<WowUnit>(WowInterface.ObjectManager.Player.Position, 100.0);

            if (nearEnemies.Any())
            {
                AmeisenLogger.I.Log("TARGET", $"2 | Targetable Units: {JsonConvert.SerializeObject(nearEnemies.Select(e => e.Name).ToList())}");
                possibleTargets = nearEnemies;
                return true;
            }

            AmeisenLogger.I.Log("TARGET", $"N | No Targetable Units");
            possibleTargets = null;
            return false;
        }
    }
}