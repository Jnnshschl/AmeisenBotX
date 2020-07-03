namespace AmeisenBotX.Core.Movement.Settings
{
    public class MovementSettings
    {
        public bool EnableDistanceMovedJumpCheck { get; set; } = true;

        public bool EnableTracelineJumpCheck { get; set; } = true;

        public float JumpCheckDistance { get; set; } = 0.2f;

        public float JumpCheckHeight { get; set; } = 0.5f;

        public float MaxAcceleration { get; set; } = 2f;

        public double MaxDistanceMovedJumpUnstuck { get; set; } = 0.2;

        public float MaxSteering { get; set; } = 1f;

        public float MaxVelocity { get; set; } = 8f;

        public double MinDistanceMovedJumpUnstuck { get; set; } = 0.0;

        public double MinUnstuckDistance { get; set; } = 8.0;

        public double SeperationDistance { get; set; } = 4.0;

        public int StuckCounterUnstuck { get; set; } = 1;

        public double WaypointCheckThreshold { get; set; } = 1.5;
    }
}