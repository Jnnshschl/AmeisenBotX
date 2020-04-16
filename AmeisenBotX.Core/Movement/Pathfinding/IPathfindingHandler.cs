using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Movement.Pathfinding
{
    public interface IPathfindingHandler
    {
        bool CastMovementRay(int mapId, Vector3 start, Vector3 end);

        List<Vector3> GetPath(int mapId, Vector3 start, Vector3 end);

        Vector3 MoveAlongSurface(int mapId, Vector3 start, Vector3 end);
    }
}