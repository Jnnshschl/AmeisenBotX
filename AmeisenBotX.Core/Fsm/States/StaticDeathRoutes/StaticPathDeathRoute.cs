using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.Fsm.States.StaticDeathRoutes
{
    public abstract class StaticPathDeathRoute : IStaticDeathRoute
    {
        private int CurrentNode { get; set; } = 0;

        protected abstract List<Vector3> Path { get; }

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

            return new(0, 0, 0);
        }

        public void Init()
        {
            CurrentNode = 0;
        }

        public bool IsUseable(WowMapId mapId, Vector3 start, Vector3 end)
        {
            return mapId == WowMapId.Northrend && ((start.GetDistance(Path[0]) < 10.0f && end.GetDistance(Path[^1]) < 10.0f) || end.GetDistance(new(5670, 2003, -100000)) < 16.0f);
        }
    }
}
