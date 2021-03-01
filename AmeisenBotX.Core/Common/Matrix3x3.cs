using AmeisenBotX.Core.Movement.Pathfinding.Objects;

namespace AmeisenBotX.Core.Common
{
    public struct Matrix3x3
    {
        public Matrix3x3(float x1, float x2, float x3, float y1, float y2, float y3, float z1, float z2, float z3)
        {
            X1 = x1;
            X2 = x2;
            X3 = x3;
            Y1 = y1;
            Y2 = y2;
            Y3 = y3;
            Z1 = z1;
            Z2 = z2;
            Z3 = z3;
        }

        public Vector3 FirstCol => new(X1, Y1, Z1);

        public float X1 { get; set; }

        public float X2 { get; set; }

        public float X3 { get; set; }

        public float Y1 { get; set; }

        public float Y2 { get; set; }

        public float Y3 { get; set; }

        public float Z1 { get; set; }

        public float Z2 { get; set; }

        public float Z3 { get; set; }

        public static Vector3 operator *(Vector3 v, Matrix3x3 m)
        {
            return new Vector3(m.X1 * v.X + m.Y1 * v.Y + m.Z1 * v.Z,
                               m.X2 * v.X + m.Y2 * v.Y + m.Z2 * v.Z,
                               m.X3 * v.X + m.Y3 * v.Y + m.Z3 * v.Z);
        }

        public float Dot()
        {
            return (X1 * Y2 * Z3) + (X2 * Y3 * Z1) + (X3 * Y1 * Z2)
                 - (X3 * Y2 * Z1) - (X2 * Y1 * Z3) - (X1 * Y3 * Z2);
        }

        public Matrix3x3 Inverse()
        {
            float d = 1 / Dot();
            return new(d * (Y2 * Z3 - Y3 * Z2), d * (X3 * Z2 - X2 * Z3), d * (X2 * Y3 - X3 * Y2),
                       d * (Y3 * Z1 - Y1 * Z3), d * (X1 * Z3 - X3 * Z1), d * (X3 * Y1 - X1 * Y3),
                       d * (Y1 * Z2 - Y2 * Z1), d * (X2 * Z1 - X1 * Z2), d * (X1 * Y2 - X2 * Y1));
        }
    }
}