using System.Numerics;

namespace AmeisenBotX.Core.Movement.Pathfinding.Objects
{
    public static class VectorExtensions
    {
        public static float GetDistance2D(this Vector3 a, Vector3 b)
        {
            a.Z = 0f;
            b.Z = 0f;
            return Vector3.DistanceSquared(a, b);
        }

        public static float GetDistance(this Vector3 a, Vector3 b)
        {
            return Vector3.DistanceSquared(a, b);
        }
    }
}
