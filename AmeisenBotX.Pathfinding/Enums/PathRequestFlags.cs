using System;

namespace AmeisenBotX.Pathfinding.Enums
{
    [Flags]
    public enum PathRequestFlags
    {
        None = 0,
        ChaikinCurve = 1 << 0,
        NaturalSteeringBehavior = 1 << 1
    }
}
