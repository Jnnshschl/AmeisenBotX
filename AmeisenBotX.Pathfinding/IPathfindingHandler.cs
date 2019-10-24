using AmeisenBotX.Pathfinding.Objects;
using System.Collections.Generic;

namespace AmeisenBotX.Pathfinding
{
    public interface IPathfindingHandler
    {
        List<Vector3> GetPath(int mapId, Vector3 start, Vector3 end);
    }
}