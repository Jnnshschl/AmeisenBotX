using AmeisenBotX.Common.Math;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Combat.Helpers.Targets.Logics
{
    public abstract class BasicTargetSelectionLogic
    {
        public BasicTargetSelectionLogic(AmeisenBotInterfaces bot)
        {
            Bot = bot;
        }

        public IEnumerable<int> BlacklistedTargets { get; set; }

        public AmeisenBotInterfaces Bot { get; }

        public IEnumerable<int> PriorityTargets { get; set; }

        public abstract bool SelectTarget(out IEnumerable<IWowUnit> wowUnit);

        protected bool IsBlacklisted(IWowUnit wowUnit)
        {
            return BlacklistedTargets != null && BlacklistedTargets.Contains(wowUnit.DisplayId);
        }

        protected bool IsPriorityTarget(IWowUnit wowUnit)
        {
            return PriorityTargets != null && PriorityTargets.Contains(wowUnit.DisplayId);
        }

        protected bool IsValidTarget(IWowUnit wowUnit)
        {
            return IWowUnit.IsValidAliveInCombat(wowUnit)
                && !IsBlacklisted(wowUnit)
                && wowUnit.DistanceTo(Bot.Player) < 80.0f
                && IsReachable(wowUnit);
        }

        protected bool IsValidEnemy(IWowUnit wowUnit)
        {
            return IsValidTarget(wowUnit)
                && Bot.Db.GetReaction(Bot.Player, wowUnit) is WowUnitReaction.Hostile or WowUnitReaction.Neutral;
        }

        protected bool IsReachable(IWowUnit wowUnit)
        {
            if (Bot.CombatClass.IsMelee)
            {
                IEnumerable<Vector3> pathToUnit = Bot.PathfindingHandler.GetPath((int)Bot.Objects.MapId, Bot.Objects.Player.Position, wowUnit.Position);

                if (pathToUnit == null || !pathToUnit.Any())
                {
                    return false;
                }

                Vector3 last = pathToUnit.Last();
                // last node should be in combat reach and not too far above
                return last.GetDistance2D(wowUnit.Position) < (wowUnit.CombatReach + 1.0f)
                    && MathF.Abs(last.Z - wowUnit.Position.Z) < 2.5f;
            }
            else
            {
                // TODO: best way should be line of sight test?
                // skipped at the moment due to too much calls
                return true;
            }
        }
    }
}