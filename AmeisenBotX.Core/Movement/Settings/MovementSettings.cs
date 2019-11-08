namespace AmeisenBotX.Core.Movement.Settings
{
    public class MovementSettings
    {
        public float DistanceToTarget { get; set; } = 3.0f;

        public float MaxVelocity { get; set; } = 9.0f;

        public double WaypointDoneThreshold { get; set; } = 7;
    }
}
