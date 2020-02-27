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

        public static Vector3 operator *(Vector3 a, float b)
            => new Vector3(a.X * b, a.Y * b, a.Z * b);

        public static Vector3 operator /(Vector3 a, float b)
            => new Vector3(a.X / b, a.Y / b, a.Z / b);

        public static Vector3 operator +(Vector3 a, Vector3 b)
            => new Vector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

        public static bool operator ==(Vector3 left, Vector3 right)
        {
            return left.Equals(right);
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

        public float GetMagnitude()
            => Convert.ToSingle(Math.Sqrt(X * X + Y * Y));

        public void Limit(float maxSpeed)
        {
            X = X < 0 ? X <= maxSpeed * -1 ? maxSpeed * -1 : X : X >= maxSpeed ? maxSpeed : X;
            Y = Y < 0 ? Y <= maxSpeed * -1 ? maxSpeed * -1 : Y : Y >= maxSpeed ? maxSpeed : Y;
            Z = Z < 0 ? Z <= maxSpeed * -1 ? maxSpeed * -1 : Z : Z >= maxSpeed ? maxSpeed : Z;
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
            => Normalize(GetMagnitude());

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
            => new float[3]
            {
                X,
                Y,
                Z
            };
    }
}