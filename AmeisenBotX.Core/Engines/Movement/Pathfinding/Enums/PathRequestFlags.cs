using System;

namespace AmeisenBotX.Core.Engines.Movement.Pathfinding.Enums
{
    [Flags]
    public enum PathRequestFlags
    {
        None = 0,
        ChaikinCurve = 1,
        CatmullRomSpline = 2,
    }
}