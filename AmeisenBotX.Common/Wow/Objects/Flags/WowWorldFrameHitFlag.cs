using System;

namespace AmeisenBotX.Wow.Objects.Flags
{
    [Flags]
    public enum WowWorldFrameHitFlag : uint
    {
        HitTestNothing = 0x0,
        HitTestBoundingModels = 0x1,
        HitTestWMO = 0x10,
        HitTestWMONoCam = 0x40,
        HitTestUnknown1 = 0x80,
        HitTestGround = 0x100,
        HitTestLiquid = 0x10000,
        HitTestLiquid2 = 0x20000,
        HitTestMovableObjects = 0x100000,
        HitTestUnknown2 = 0x200000,
        HitTestUnknown3 = 0x400000,
        HitTestLOS = 0x100011,
        HitTestGroundAndStructures = 0x100111,
    }
}