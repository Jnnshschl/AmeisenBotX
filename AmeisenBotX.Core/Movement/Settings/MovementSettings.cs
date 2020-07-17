namespace AmeisenBotX.Core.Movement.Settings
{
    public class MovementSettings
    {
        public bool EnableDistanceMovedJumpCheck { get; set; } = true;

        public bool EnableTracelineJumpCheck { get; set; } = true;

        public float JumpCheckDistance { get; set; } = 0.3f;

        public float JumpCheckHeight { get; set; } = 0.3f;

        public float MaxAcceleration { get; set; } = 1.0f;

        public double MaxDistanceMovedJumpUnstuck { get; set; } = 0.2;

        public float MaxSteering { get; set; } = 1.0f;

        public float MaxVelocity { get; set; } = 4f;

        public double MinDistanceMovedJumpUnstuck { get; set; } = 0.0;

        public double MinUnstuckDistance { get; set; } = 8.0;

        public double SeperationDistance { get; set; } = 4.0;

        public int StuckCounterUnstuck { get; set; } = 3;

        public float UnstuckDistance { get; set; } = 6.0f;

        public double WaypointCheckThreshold { get; set; } = 2.0;
    }
}