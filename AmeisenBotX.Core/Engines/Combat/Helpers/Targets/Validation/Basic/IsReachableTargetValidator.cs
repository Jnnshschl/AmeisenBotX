using AmeisenBotX.Common.Math;
using AmeisenBotX.Wow.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Combat.Helpers.Targets.Validation.Basic
{
    public class IsReachableTargetValidator(AmeisenBotInterfaces bot, float maxDistance = 80.0f) : ITargetValidator
    {
        private AmeisenBotInterfaces Bot { get; } = bot;

        private float MaxDistance { get; } = maxDistance;

        public bool IsValid(IWowUnit unit)
        {
            // unit needs to be reachable (path end must be in combat reach for melees) and the path
            // should be shorter than MaxDistance
            return IsInRangePathfinding(unit);
        }

        private bool HasPathReachedUnit(Vector3 position, IWowUnit unit)
        {
            if (Bot.CombatClass.IsMelee)
            {
                // last node should be in combat reach and not too far above
                return position.GetDistance2D(unit.Position) <= Bot.Player.MeleeRangeTo(unit)
                    && MathF.Abs(position.Z - unit.Position.Z) < 2.5f;
            }
            else
            {
                // TODO: best way should be line of sight test? skipped at the moment due to too
                // much calls
                return Bot.Player.DistanceTo(unit) <= 40.0f;
            }
        }

        private bool IsInRangePathfinding(IWowUnit unit)
        {
            float distance = 0;
            IEnumerable<Vector3> path = Bot.PathfindingHandler.GetPath((int)Bot.Objects.MapId, Bot.Objects.Player.Position, unit.Position);

            if (path != null && path.Any() && HasPathReachedUnit(path.Last(), unit))
            {
                for (int i = 0; i < path.Count() - 1; ++i)
                {
                    distance += path.ElementAt(i).GetDistance(path.ElementAt(i + 1));

                    if (distance > MaxDistance)
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }
    }
}