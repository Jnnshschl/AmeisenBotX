namespace AmeisenBotX.Core.Movement.Settings
{
    public class MovementSettings
    {
        public float MaxAcceleration { get; set; } = 3.5f;

        public float MaxSteering { get; set; } = 1.4f;

        public float MaxVelocity { get; set; } = 7f;

        public double SeperationDistance { get; set; } = 8;

        public double WaypointCheckThreshold { get; set; } = 4;
    }
}