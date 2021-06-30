using AmeisenBotX.Common.Math;
using AmeisenBotX.Wow.Objects.Enums;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Fsm.States.StaticDeathRoutes
{
    /// <summary>
    /// Static death routes are used to describe routes to dungeons that cannot be reached by the bots pathfinding at the moment.
    /// </summary>
    public abstract class StaticPathDeathRoute : IStaticDeathRoute
    {
        /// <summary>
        /// Set this to the "fake" corpse position that lies beneath the dungeon.
        /// </summary>
        protected abstract Vector3 DeathPoint { get; }

        /// <summary>
        /// Static path from the graveyard the the dungeon entry.
        /// </summary>
        protected abstract List<Vector3> Path { get; }

        private int CurrentNode { get; set; } = 0;

        /// <inheritdoc cref="IStaticDeathRoute.GetNextPoint(Vector3)"/>
        public Vector3 GetNextPoint(Vector3 playerPosition)
        {
            if (CurrentNode < Path.Count)
            {
                if (playerPosition.GetDistance(Path[CurrentNode]) < 3.0f)
                {
                    ++CurrentNode;
                    return GetNextPoint(playerPosition);
                }

                return Path[CurrentNode];
            }
            else
            {
                CurrentNode = 0;
            }

            return new(0, 0, 0);
        }

        /// <inheritdoc cref="IStaticDeathRoute.Init(Vector3)"/>
        public void Init(Vector3 playerPosition)
        {
            CurrentNode = Path.IndexOf(Path.OrderBy(e => e.GetDistance(playerPosition)).First());
        }

        /// <inheritdoc cref="IStaticDeathRoute.IsUseable(WowMapId, Vector3, Vector3)"/>
        public bool IsUseable(WowMapId mapId, Vector3 start, Vector3 end)
        {
            return mapId == WowMapId.Northrend && ((start.GetDistance(Path[0]) < 4.0f && end.GetDistance(Path[^1]) < 4.0f) || end.GetDistance(DeathPoint) < 5.0f);
        }
    }
}