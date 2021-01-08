using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Movement.Pathfinding
{
    public interface IPathfindingHandler
    {
        IEnumerable<Vector3> GetPath(int mapId, Vector3 origin, Vector3 target);

        double GetPathDistance(int mapId, Vector3 origin, Vector3 target);

        double GetPathDistance(IEnumerable<Vector3> path, Vector3 start);

        Vector3 GetRandomPoint(int mapId);

        Vector3 GetRandomPointAround(int mapId, Vector3 origin, float maxRadius);

        Vector3 MoveAlongSurface(int mapId, Vector3 origin, Vector3 target);
    }
}