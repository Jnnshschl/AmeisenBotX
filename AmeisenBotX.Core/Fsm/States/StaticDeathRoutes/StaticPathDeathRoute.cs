using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Fsm.States.StaticDeathRoutes
{
    public abstract class StaticPathDeathRoute : IStaticDeathRoute
    {
        protected abstract Vector3 DeathPoint { get; }

        protected abstract List<Vector3> Path { get; }

        private int CurrentNode { get; set; } = 0;

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

        public void Init(Vector3 playerPosition)
        {
            CurrentNode = Path.IndexOf(Path.OrderBy(e=>e.GetDistance(playerPosition)).First());
        }

        public bool IsUseable(WowMapId mapId, Vector3 start, Vector3 end)
        {
            return mapId == WowMapId.Northrend && ((start.GetDistance(Path[0]) < 4.0f && end.GetDistance(Path[^1]) < 4.0f) || end.GetDistance(DeathPoint) < 5.0f);
        }
    }
}