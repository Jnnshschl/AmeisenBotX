using System.Collections.Generic;

namespace AmeisenBotX.Pathfinding
{
    public interface IPathfindingHandler
    {
        List<WowPosition> GetPath(int mapId, WowPosition start, WowPosition end);
    }
}