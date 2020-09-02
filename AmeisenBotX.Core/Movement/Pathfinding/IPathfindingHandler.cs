using System.Collections.Generic;
using System.Numerics;

namespace AmeisenBotX.Core.Movement.Pathfinding
{
    public interface IPathfindingHandler
    {
        bool CastMovementRay(int mapId, Vector3 origin, Vector3 target);

        IEnumerable<Vector3> GetPath(int mapId, Vector3 origin, Vector3 target);

        Vector3 GetRandomPoint(int mapId);

        Vector3 GetRandomPointAround(int mapId, Vector3 origin, float maxRadius);

        Vector3 MoveAlongSurface(int mapId, Vector3 origin, Vector3 target);
    }
}