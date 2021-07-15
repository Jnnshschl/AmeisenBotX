namespace AmeisenBotX.Core.Engines.Movement.Settings
{
    public class MovementSettings
    {
        public bool EnableDistanceMovedJumpCheck { get; set; } = true;

        public float MaxAcceleration { get; set; } = 0.32f;

        public float MaxAccelerationCombat { get; set; } = 2f;

        public double MaxDistanceMovedJumpUnstuck { get; set; } = 1.5;

        public float MaxSteering { get; set; } = 0.7f;

        public float MaxSteeringCombat { get; set; } = 1f;

        public float MaxVelocity { get; set; } = 4f;

        public double MinDistanceMovedJumpUnstuck { get; set; } = -1.0;

        public double MinUnstuckDistance { get; set; } = 8.0;

        public float SeperationDistance { get; set; } = 2.0f;

        public int StuckCounterUnstuck { get; set; } = 3;

        public float UnstuckDistance { get; set; } = 6.0f;

        public double WaypointCheckThreshold { get; set; } = 1.3;

        public double WaypointCheckThresholdMounted { get; set; } = 3.0;
    }
}