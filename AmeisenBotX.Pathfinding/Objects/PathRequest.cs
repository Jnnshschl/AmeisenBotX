using AmeisenBotX.Pathfinding.Enums;

namespace AmeisenBotX.Pathfinding.Objects
{
    public struct PathRequest
    {
        public PathRequest(Vector3 a, Vector3 b, int mapId, PathRequestFlags flags = PathRequestFlags.None, MovementType movementType = MovementType.MOVE_TO_POSITION)
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

        public static bool operator !=(PathRequest left, PathRequest right)
        {
            return !(left == right);
        }

        public static bool operator ==(PathRequest left, PathRequest right)
        {
            return left.Equals(right);
        }

        public override bool Equals(object obj)
            => obj.GetType() == typeof(PathRequest)
            && ((PathRequest)obj).A == A
            && ((PathRequest)obj).B == B
            && ((PathRequest)obj).MapId == MapId
            && ((PathRequest)obj).Flags == Flags
            && ((PathRequest)obj).MovementType == MovementType;

        public override int GetHashCode()
        {
            unchecked
            {
                return (int)(17 + (A.GetHashCode() * 23) + (B.GetHashCode() * 23) + (MapId * 23) + ((int)Flags * 23) + ((int)MovementType * 23));
            }
        }
    }
}