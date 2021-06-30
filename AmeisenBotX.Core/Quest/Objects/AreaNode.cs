using AmeisenBotX.Common.Math;

namespace AmeisenBotX.Core.Quest.Objects
{
    public class AreaNode
    {
        public AreaNode(Vector3 position, double radius)
        {
            Position = position;
            Radius = radius;
        }

        public Vector3 Position { get; set; }

        public double Radius { get; set; }
    }
}