using System;

namespace AmeisenBotX.Core.Movement.Pathfinding.Enums
{
    [Flags]
    public enum PathRequestFlags
    {
        None = 0,
        ChaikinCurve = 1 << 0,
    }
}