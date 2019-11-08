using System;

namespace AmeisenBotX.Pathfinding.Objects
{
    public struct Vector3
    {
        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static Vector3 Zero { get; } = new Vector3(0, 0, 0);

        public float X { get; set; }

        public float Y { get; set; }

        public float Z { get; set; }

        public static Vector3 FromArray(float[] array)
            => new Vector3()
            {
                X = array[0],
                Y = array[1],
                Z = array[2]
            };

        public static Vector3 operator -(Vector3 a, Vector3 b)
            => new Vector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

        public static bool operator !=(Vector3 left, Vector3 right)
        {
            return !(left == right);
        }

        public static Vector3 operator +(Vector3 a, Vector3 b)
            => new Vector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

        public static bool operator ==(Vector3 left, Vector3 right)
        {
            return left.Equals(right);
        }

        public override bool Equals(object obj)
            => obj.GetType() == typeof(Vector3)
            && ((Vector3)obj).X == X
            && ((Vector3)obj).Y == Y
            && ((Vector3)obj).Z == Z;

        public double GetDistance(Vector3 b)
            => Math.Sqrt(((X - b.X) * (X - b.X))
                       + ((Y - b.Y) * (Y - b.Y))
                       + ((Z - b.Z) * (Z - b.Z)));

        public double GetDistance2D(Vector3 b)
            => Math.Sqrt(Math.Pow(X - b.X, 2) + Math.Pow(Y - b.Y, 2));

        public override int GetHashCode()
        {
            unchecked
            {
                return (int)(17 + (X * 23) + (Y * 23) + (Z * 23));
            }
        }

        public float[] ToArray()
            => new float[3]
            {
                X,
                Y,
                Z
            };
    }
}
