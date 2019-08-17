using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Pathfinding
{
    public struct PathRequest
    {
        public int MapId { get; set; }
        public WowPosition A { get; set; }
        public WowPosition B { get; set; }

        public PathRequest(int mapId, WowPosition a, WowPosition b)
        {
            MapId = mapId;
            A = a;
            B = b;
        }
    }
}
