using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AmeisenBotX.Common.Math
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector3 : IEquatable<Vector3>
    {
        public static readonly Vector3 Zero = new(0, 0, 0);

        public Vector3(float a)
        {
            X = a;
            Y = a;
            Z = a;
        }

        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public readonly bool AnySubZero()
        {
            return X < 0.0f || Y < 0.0f || Z < 0.0f;
        }

        public Vector3(Vector3 v3) : this(v3.X, v3.Y, v3.Z)
        {
        }

        public Vector3(float[] array) : this(array[0], array[1], array[2])
        {
        }

        public float X { get; set; }

        public float Y { get; set; }

        public float Z { get; set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator -(Vector3 a, Vector3 b)
        {
            return new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator -(Vector3 a, float b)
        {
            return new(a.X - b, a.Y - b, a.Z - b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Vector3 a, Vector3 b)
        {
            return a.X != b.X
                && a.Y != b.Y
                && a.Z != b.Z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator *(Vector3 a, float b)
        {
            return new(a.X * b, a.Y * b, a.Z * b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator *(Vector3 a, Vector3 b)
        {
            return new(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator /(Vector3 a, Vector3 b)
        {
            return new(a.X / b.X, a.Y / b.Y, a.Z / b.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator /(Vector3 a, float b)
        {
            return new(a.X / b, a.Y / b, a.Z / b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator +(Vector3 a, float b)
        {
            return new(a.X + b, a.Y + b, a.Z + b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator +(Vector3 a, Vector3 b)
        {
            return new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(Vector3 a, Vector3 b)
        {
            return a.X < b.X
                && a.Y < b.Y
                && a.Z < b.Z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Vector3 a, Vector3 b)
        {
            return a.X == b.X
                && a.Y == b.Y
                && a.Z == b.Z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(Vector3 a, Vector3 b)
        {
            return a.X > b.X
                && a.Y > b.Y
                && a.Z > b.Z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(Vector3 vector)
        {
            X += vector.X;
            Y += vector.Y;
            Z += vector.Z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(float n)
        {
            X += n;
            Y += n;
            Z += n;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Divide(Vector3 v)
        {
            X = v.X > 0f ? X / v.X : 0f;
            Y = v.Y > 0f ? Y / v.Y : 0f;
            Z = v.Z > 0f ? Z / v.Z : 0f;
        }

        public Vector3 Truncated(float max)
        {
            Truncate(max);
            return this;
        }

        public Vector3 Limited(float limit)
        {
            Limit(limit);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Divide(float n)
        {
            X = n > 0f ? X / n : 0f;
            Y = n > 0f ? Y / n : 0f;
            Z = n > 0f ? Z / n : 0f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 Normalized()
        {
            return Normalized(GetMagnitude());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 Normalized(float magnitude)
        {
            return magnitude > 0.0f
                ? new
                (
                    X /= magnitude,
                    Y /= magnitude,
                    Z /= magnitude
                )
                : this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 ZeroZ()
        {
            return AdjustedZ(0.0f);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 AdjustedZ(float z)
        {
            Z = z;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float GetDistance(Vector3 v)
        {
            return MathF.Sqrt(MathF.Pow(X - v.X, 2) + MathF.Pow(Y - v.Y, 2) + MathF.Pow(Z - v.Z, 2));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float GetDistance2D(Vector3 v)
        {
            return MathF.Sqrt(MathF.Pow(X - v.X, 2) + MathF.Pow(Y - v.Y, 2));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override readonly int GetHashCode()
        {
            unchecked
            {
                return (int)(17 + (X * 23) + (Y * 23) + (Z * 23));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float Dot()
        {
            return MathF.Pow(X, 2) + MathF.Pow(Y, 2) + MathF.Pow(Z, 2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float Dot2D()
        {
            return MathF.Pow(X, 2) + MathF.Pow(Y, 2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float GetMagnitude()
        {
            return MathF.Sqrt(Dot());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float GetMagnitude2D()
        {
            return MathF.Sqrt(Dot2D());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Limit(float limit)
        {
            X = X < 0f ? MathF.Max(X, limit * -1f) : MathF.Min(X, limit);
            Y = Y < 0f ? MathF.Max(Y, limit * -1f) : MathF.Min(Y, limit);
            Z = Z < 0f ? MathF.Max(Z, limit * -1f) : MathF.Min(Z, limit);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Truncate(float max)
        {
            float factor = max / GetMagnitude();
            Scale(factor < 1.0f ? factor : 1.0f);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Scale(float factor)
        {
            X *= factor;
            Y *= factor;
            Z *= factor;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Multiply(Vector3 vector)
        {
            X = vector.X > 0f ? X * vector.X : 0f;
            Y = vector.Y > 0f ? Y * vector.Y : 0f;
            Z = vector.Z > 0f ? Z * vector.Z : 0f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Multiply(float n)
        {
            X = n > 0f ? X * n : 0f;
            Y = n > 0f ? Y * n : 0f;
            Z = n > 0f ? Z * n : 0f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Normalize()
        {
            Normalize(GetMagnitude());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Normalize(float magnitude)
        {
            if (magnitude > 0f)
            {
                X /= magnitude;
                Y /= magnitude;
                Z /= magnitude;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Normalize2D()
        {
            Normalize2D(GetMagnitude2D());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Normalize2D(float magnitude)
        {
            if (magnitude > 0)
            {
                X /= magnitude;
                Y /= magnitude;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Rotate(float degrees)
        {
            RotateRadians(degrees * (MathF.PI / 180f));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RotateRadians(float radians)
        {
            float ca = MathF.Cos(radians);
            float sa = MathF.Sin(radians);

            X = ca * X - sa * Y;
            Y = sa * X + ca * Y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Subtract(float n)
        {
            X -= n;
            Y -= n;
            Z -= n;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Subtract(Vector3 vector)
        {
            X -= vector.X;
            Y -= vector.Y;
            Z -= vector.Z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float[] ToArray()
        {
            return [X, Y, Z];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override readonly string ToString()
        {
            return $"{X}, {Y}, {Z}";
        }

        public override readonly bool Equals(object obj)
        {
            return obj is Vector3 vector && this == vector;
        }

        public readonly bool Equals(Vector3 other)
        {
            return this == other;
        }
    }
}