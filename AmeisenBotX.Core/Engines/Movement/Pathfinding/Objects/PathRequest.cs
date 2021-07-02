using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Movement.Pathfinding.Enums;
using System.Runtime.InteropServices;

namespace AmeisenBotX.Core.Engines.Movement.Pathfinding.Objects
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PathRequest
    {
        public PathRequest(int mapId, Vector3 a, Vector3 b, PathRequestFlags flags = PathRequestFlags.None, MovementType movementType = MovementType.FindPath)
        {
            Type = 1;
            A = a;
            B = b;
            MapId = mapId;
            Flags = flags;
            MovementType = movementType;
        }

        public int Type { get; }

        public Vector3 A { get; set; }

        public Vector3 B { get; set; }

        public PathRequestFlags Flags { get; set; }

        public int MapId { get; set; }

        public MovementType MovementType { get; set; }
    }
}