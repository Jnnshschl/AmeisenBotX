using System;

namespace AmeisenBotX.Wow.Objects.Flags
{
    [Flags]
    public enum WowGameobjectFlags : int
    {
        InUse = 0x1,                  // HEX: 0x0000 0001 - DEC: 1
        Locked = 0x2,                 // HEX: 0x0000 0002 - DEC: 2
        ConditionalInteraction = 0x4, // HEX: 0x0000 0004 - DEC: 4
        Transport = 0x8,              // HEX: 0x0000 0008 - DEC: 8
        NotSelectable = 0x10,         // HEX: 0x0000 0010 - DEC: 16
        NoDespawn = 0x20,             // HEX: 0x0000 0020 - DEC: 32
        AIObstacle = 0x40,            // HEX: 0x0000 0040 - DEC: 64
        FreezeAnimation = 0x80,       // HEX: 0x0000 0080 - DEC: 128
        Damaged = 0x200,              // HEX: 0x0000 0200 - DEC: 512
        Destroyed = 0x400,            // HEX: 0x0000 0400 - DEC: 1024
        Unknown1 = 0x800,             // HEX: 0x0000 0800 - DEC: 2048
        Unknown2 = 0x1000,            // HEX: 0x0000 1000 - DEC: 4096
        Unknown3 = 0x2000,            // HEX: 0x0000 2000 - DEC: 8192
        Unknown4 = 0x4000,            // HEX: 0x0000 4000 - DEC: 16384
        Unknown5 = 0x8000,            // HEX: 0x0000 8000 - DEC: 32768
    }
}