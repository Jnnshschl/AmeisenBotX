namespace AmeisenBotX.Core.Movement.Settings
{
    public class MovementSettings
    {
        public float MaxAcceleration { get; set; } = 1f;

        public float MaxSteering { get; set; } = 0.7f;

        public float MaxVelocity { get; set; } = 5f;

        public double SeperationDistance { get; set; } = 4;

        public double WaypointCheckThreshold { get; set; } = 4;
    }
}