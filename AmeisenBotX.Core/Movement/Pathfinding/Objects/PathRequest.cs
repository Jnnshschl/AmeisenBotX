using AmeisenBotX.Core.Movement.Pathfinding.Enums;

namespace AmeisenBotX.Core.Movement.Pathfinding.Objects
{
    public struct PathRequest
    {
        public PathRequest(int mapId, Vector3 a, Vector3 b, PathRequestFlags flags = PathRequestFlags.None, MovementType movementType = MovementType.FindPath)
        {
            A = a;
            B = b;
            MapId = mapId;
            Flags = flags;
            MovementType = movementType;
        }

        public Vector3 A { get; set; }

        public Vector3 B { get; set; }

        public PathRequestFlags Flags { get; set; }

        public int MapId { get; set; }

        public MovementType MovementType { get; set; }
    }
}