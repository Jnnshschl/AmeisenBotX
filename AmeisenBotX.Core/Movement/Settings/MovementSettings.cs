using AmeisenBotX.Pathfinding.Objects;
using System;

namespace AmeisenBotX.Core.Movement.Settings
{
    public class MovementSettings
    {
        public MovementSettings()
        {
            //// Random rnd = new Random();
            //// MaxSpeed *= Convert.ToSingle(rnd.NextDouble() + 1);
            //// MaxForce *= Convert.ToSingle(rnd.NextDouble() + 1);
            //// Acceleration *= Convert.ToSingle(rnd.NextDouble() + 1);
            //// WaypointCheckThreshold *= rnd.NextDouble() + 1;
            //// SeperationDistance *= rnd.NextDouble() + 1;
        }

        public float MaxVelocity { get; set; } = 5f;

        public float MaxSteering { get; set; } = 1f;

        public float MaxAcceleration { get; set; } = 3f;

        public double WaypointCheckThreshold { get; set; } = 2;

        public double SeperationDistance { get; set; } = 4;
    }
}
