using AmeisenBotX.Common.Math;

namespace AmeisenBotX.Core.Engines.Quest.Objects
{
    public class AreaNode(Vector3 position, double radius)
    {
        public Vector3 Position { get; set; } = position;

        public double Radius { get; set; } = radius;
    }
}