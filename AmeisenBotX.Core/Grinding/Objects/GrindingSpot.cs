using AmeisenBotX.Core.Movement.Pathfinding.Objects;

namespace AmeisenBotX.Core.Grinding.Objects
{
    public class GrindingSpot
    {
        public GrindingSpot()
        {
        }

        public GrindingSpot(Vector3 position, double radius)
        {
            Position = position;
            Radius = radius;
        }

        public Vector3 Position { get; set; }

        public double Radius { get; set; }
    }
}