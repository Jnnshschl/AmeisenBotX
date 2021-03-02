using AmeisenBotX.Core.Movement.Pathfinding.Objects;

namespace AmeisenBotX.Core.Grinding.Objects
{
    public class GrindingSpot
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

        public int MaxLevel { get; set; }

        public int MinLevel { get; set; }

        public Vector3 Position { get; set; }

        public float Radius { get; set; }
    }
}