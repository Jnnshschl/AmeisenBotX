using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Pathfinding
{
    public interface IPathfindingHandler
    {
        List<WowPosition> GetPath(int mapId, WowPosition start, WowPosition end);
    }
}
