using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Pathfinding
{
    public struct WowPosition
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public double GetDistance(WowPosition b)
            => Math.Sqrt(((X - b.X) * (X - b.X))
                       + ((Y - b.Y) * (Y - b.Y))
                       + ((Z - b.Z) * (Z - b.Z)));
        public double GetDistance2D(WowPosition b)
            => Math.Sqrt(Math.Pow(X - b.X, 2) + Math.Pow(Y - b.Y, 2));
    }
}
