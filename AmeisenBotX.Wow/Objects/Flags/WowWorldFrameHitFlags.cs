using System;

namespace AmeisenBotX.Wow.Objects.Flags
{
    [Flags]
    public enum WowWorldFrameHitFlags : uint
    {
        HitTestNothing = 0x0,                  // HEX: 0x0000 0000 - DEC: 0
        HitTestBoundingModels = 0x1,           // HEX: 0x0000 0001 - DEC: 1
        HitTestWMO = 0x10,                     // HEX: 0x0000 0010 - DEC: 16
        HitTestWMONoCam = 0x40,                // HEX: 0x0000 0040 - DEC: 64
        HitTestUnknown1 = 0x80,                // HEX: 0x0000 0080 - DEC: 128
        HitTestGround = 0x100,                 // HEX: 0x0000 0100 - DEC: 256
        HitTestLiquid = 0x10000,               // HEX: 0x0001 0000 - DEC: 65536
        HitTestLiquid2 = 0x20000,              // HEX: 0x0002 0000 - DEC: 131072
        HitTestMovableObjects = 0x100000,      // HEX: 0x0010 0000 - DEC: 1048576
        HitTestUnknown2 = 0x200000,            // HEX: 0x0020 0000 - DEC: 2097152
        HitTestUnknown3 = 0x400000,            // HEX: 0x0040 0000 - DEC: 4194304
        // ------- Composite flags ------>
        HitTestLOS = 0x100011,                 // HEX: 0x0010 0011 - DEC: 1048593; HitTestMovableObjects + HitTestWMO + HitTestBoundingModels
        HitTestGroundAndStructures = 0x100111, // HEX: 0x0010 0111 - DEC: 1048849; HitTestLOS + HitTestGround
    }
}