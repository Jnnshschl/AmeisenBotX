namespace AmeisenBotX.Core.Movement.Settings
{
    public class MovementSettings
    {
        public float MaxAcceleration { get; set; } = 3f;

        public float MaxSteering { get; set; } = 2f;

        public float MaxVelocity { get; set; } = 6f;

        public double SeperationDistance { get; set; } = 2;

        public double WaypointCheckThreshold { get; set; } = 5;
    }
}