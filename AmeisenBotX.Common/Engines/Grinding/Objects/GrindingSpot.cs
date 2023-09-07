using AmeisenBotX.Common.Math;

namespace AmeisenBotX.Core.Engines.Grinding.Objects
{
    public struct GrindingSpot
    {
        public GrindingSpot()
        {
        }

        public GrindingSpot(Vector3 position, float radius, int minLevel, int maxLevel)
        {
            Position = position;
            Radius = radius;
            MinLevel = minLevel;
            MaxLevel = maxLevel;
        }

        public int MaxLevel { get; set; } = 0;

        public int MinLevel { get; set; } = 0;

        public Vector3 Position { get; set; } = new();

        public float Radius { get; set; } = 0;
    }
}