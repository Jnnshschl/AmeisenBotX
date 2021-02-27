using System;

namespace AmeisenBotX.Core.Data.Enums
{
    [Flags]
    public enum WowWorldFrameHitFlags : uint
    {
        HitTestNothing = 0x0,
        HitTestBoundingModels = 0x1,
        HitTestWMO = 0x10,
        HitTestUnknown = 0x40,
        HitTestGround = 0x100,
        HitTestLiquid = 0x10000,
        HitTestUnknown2 = 0x20000,
        HitTestMovableObjects = 0x100000,

        HitTestLOS = HitTestBoundingModels | HitTestWMO,
        HitTestGroundAndStructures = HitTestLOS | HitTestGround
    }
}