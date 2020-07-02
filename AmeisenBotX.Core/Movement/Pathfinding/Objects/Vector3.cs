using System;
using System.Runtime.CompilerServices;

namespace AmeisenBotX.Core.Movement.Pathfinding.Objects
{
    [Serializable]
    public struct Vector3
    {
        public static readonly Vector3 Zero = new Vector3(0, 0, 0);

        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Vector3(Vector3 position) : this(position.X, position.Y, position.Z)
        {
        }

        public float X { get; set; }

        public float Y { get; set; }

        public float Z { get; set; }

        public static Vector3 FromArray(float[] array)
        {
            return new Vector3()
            {
                X = array[0],
                Y = array[1],
                Z = array[2]
            };
        }

        public static Vector3 operator -(Vector3 a, Vector3 b)
        {
            return new Vector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public static bool operator !=(Vector3 left, Vector3 right)
        {
            return !(left == right);
        }

        public static Vector3 operator *(Vector3 a, float b)
        {
            return new Vector3(a.X * b, a.Y * b, a.Z * b);
        }

        public static Vector3 operator *(Vector3 a, Vector3 b)
        {
            return new Vector3(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        }

        public static Vector3 operator /(Vector3 a, Vector3 b)
        {
            return new Vector3(a.X / b.X, a.Y / b.Y, a.Z / b.Z);
        }

        public static Vector3 operator /(Vector3 a, float b)
        {
            return new Vector3(a.X / b, a.Y / b, a.Z / b);
        }

        public static Vector3 operator +(Vector3 a, Vector3 b)
        {
            return new Vector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static bool operator <(Vector3 left, Vector3 right)
        {
            return left.X < right.X
                       && left.Y < right.Y
                       && left.Z < right.Z;
        }

        public static bool operator ==(Vector3 left, Vector3 right)
        {
            return left.Equals(right);
        }

        public static bool operator >(Vector3 left, Vector3 right)
        {
            return left.X > right.X
                       && left.Y > right.Y
                       && left.Z > right.Z;
        }

        public void Add(Vector3 vector)
        {
            X += vector.X;
            Y += vector.Y;
            Z += vector.Z;
        }

        public void Divide(Vector3 vector)
        {
            X = vector.X > 0 ? X / vector.X : 0;
            Y = vector.Y > 0 ? Y / vector.Y : 0;
            Z = vector.Z > 0 ? Z / vector.Z : 0;
        }

        public void Divide(float multiplier)
        {
            X = multiplier > 0 ? X / multiplier : 0;
            Y = multiplier > 0 ? Y / multiplier : 0;
            Z = multiplier > 0 ? Z / multiplier : 0;
        }

        public override bool Equals(object obj)
        {
            return obj.GetType() == typeof(Vector3)
                       && ((Vector3)obj).X == X
                       && ((Vector3)obj).Y == Y
                       && ((Vector3)obj).Z == Z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double GetDistance(Vector3 b)
        {
            return Math.Sqrt(Math.Pow((X - b.X), 2) + Math.Pow((Y - b.Y), 2) + Math.Pow((Z - b.Z), 2));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double GetDistanceIgnoreZ(Vector3 b)
        {
            return Math.Sqrt(Math.Pow((X - b.X), 2) + Math.Pow((Y - b.Y), 2));
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (int)(17 + (X * 23) + (Y * 23) + (Z * 23));
            }
        }

        public float GetMagnitude()
        {
            return Convert.ToSingle(Math.Sqrt(X * X + Y * Y));
        }

        public void Limit(float maxSpeed)
        {
            X = X < 0f ? Math.Max(X, maxSpeed * -1) : Math.Min(X, maxSpeed);
            Y = Y < 0f ? Math.Max(Y, maxSpeed * -1) : Math.Min(Y, maxSpeed);
            Z = Z < 0f ? Math.Max(Z, maxSpeed * -1) : Math.Min(Z, maxSpeed);
        }

        public void Multiply(Vector3 vector)
        {
            X = vector.X > 0 ? X * vector.X : 0;
            Y = vector.Y > 0 ? Y * vector.Y : 0;
            Z = vector.Z > 0 ? Z * vector.Z : 0;
        }

        public void Multiply(float multiplier)
        {
            X = multiplier > 0 ? X * multiplier : 0;
            Y = multiplier > 0 ? Y * multiplier : 0;
            Z = multiplier > 0 ? Z * multiplier : 0;
        }

        public void Normalize()
        {
            Normalize(GetMagnitude());
        }

        public void Normalize(float magnitude)
        {
            if (magnitude > 0)
            {
                X /= magnitude;
                Y /= magnitude;
            }
        }

        public void Rotate(double degrees)
        {
            RotateRadians(degrees * (Math.PI / 180));
        }

        public void RotateRadians(double radians)
        {
            double ca = Math.Cos(radians);
            double sa = Math.Sin(radians);

            X = Convert.ToSingle(ca * X - sa * Y);
            Y = Convert.ToSingle(sa * X + ca * Y);
        }

        public void Subtract(Vector3 vector)
        {
            X -= vector.X;
            Y -= vector.Y;
            Z -= vector.Z;
        }

        public float[] ToArray()
        {
            return new float[3] { X, Y, Z };
        }

        public override string ToString()
        {
            return $"X: {X}, Y: {Y}, Z: {Z}";
        }
    }
}