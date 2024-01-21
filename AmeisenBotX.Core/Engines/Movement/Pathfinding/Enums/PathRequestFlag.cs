using System;

namespace AmeisenBotX.Core.Engines.Movement.Pathfinding.Enums
{
    [Flags]
    public enum PathRequestFlag
    {
        None = 0,
        ChaikinCurve = 1,
        CatmullRomSpline = 2,
        BezierCurve = 4,
    }
}