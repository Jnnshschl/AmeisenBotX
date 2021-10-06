namespace AmeisenBotX.Wow.Objects.Enums
{
    public enum WowWorldFrameHit : uint
    {
        Nothing = 0x0,
        BoundingModels = 0x1,
        WMO = 0x10,
        Unk = 0x40,
        Ground = 0x100,
        Liquid = 0x10000,
        Liquid2 = 0x20000,
        MovableObjects = 0x100000,

        LineOfSight = WMO | BoundingModels | MovableObjects,
        GroundAndStructures = LineOfSight | Ground
    }
}