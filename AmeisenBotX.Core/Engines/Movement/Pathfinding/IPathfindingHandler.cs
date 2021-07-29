using AmeisenBotX.Common.Math;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Movement.Pathfinding
{
    public interface IPathfindingHandler
    {
        IEnumerable<Vector3> GetPath(int mapId, Vector3 origin, Vector3 target);

        Vector3 GetRandomPoint(int mapId);

        Vector3 GetRandomPointAround(int mapId, Vector3 origin, float maxRadius);

        Vector3 MoveAlongSurface(int mapId, Vector3 origin, Vector3 target);

        void Stop();
    }
}