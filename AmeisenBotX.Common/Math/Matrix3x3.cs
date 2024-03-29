﻿namespace AmeisenBotX.Common.Math
{
    public struct Matrix3x3(float x1, float x2, float x3, float y1, float y2, float y3, float z1, float z2, float z3)
    {
        public float X1 { get; set; } = x1;

        public float X2 { get; set; } = x2;

        public float X3 { get; set; } = x3;

        public float Y1 { get; set; } = y1;

        public float Y2 { get; set; } = y2;

        public float Y3 { get; set; } = y3;

        public float Z1 { get; set; } = z1;

        public float Z2 { get; set; } = z2;

        public float Z3 { get; set; } = z3;

        public static Vector3 operator *(Vector3 v, Matrix3x3 m)
        {
            return new Vector3(m.X1 * v.X + m.Y1 * v.Y + m.Z1 * v.Z,
                               m.X2 * v.X + m.Y2 * v.Y + m.Z2 * v.Z,
                               m.X3 * v.X + m.Y3 * v.Y + m.Z3 * v.Z);
        }

        public readonly float Dot()
        {
            return (X1 * Y2 * Z3) + (X2 * Y3 * Z1) + (X3 * Y1 * Z2)
                 - (X3 * Y2 * Z1) - (X2 * Y1 * Z3) - (X1 * Y3 * Z2);
        }

        public readonly Matrix3x3 Inverse()
        {
            float d = 1 / Dot();
            return new(d * (Y2 * Z3 - Y3 * Z2), d * (X3 * Z2 - X2 * Z3), d * (X2 * Y3 - X3 * Y2),
                       d * (Y3 * Z1 - Y1 * Z3), d * (X1 * Z3 - X3 * Z1), d * (X3 * Y1 - X1 * Y3),
                       d * (Y1 * Z2 - Y2 * Z1), d * (X2 * Z1 - X1 * Z2), d * (X1 * Y2 - X2 * Y1));
        }
    }
}