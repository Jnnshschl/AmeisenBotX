using System;

namespace AmeisenBotX.Wow.Objects.Flags
{
    [Flags]
    public enum WowSpellSchoolFlag
    {
        None = 0x0,     // HEX: 0x0000 0000 - DEC: 0
        Physical = 0x1, // HEX: 0x0000 0001 - DEC: 1
        Holy = 0x2,     // HEX: 0x0000 0002 - DEC: 2
        Fire = 0x4,     // HEX: 0x0000 0004 - DEC: 4
        Nature = 0x8,   // HEX: 0x0000 0008 - DEC: 8
        Frost = 0x10,   // HEX: 0x0000 0010 - DEC: 16
        Shadow = 0x20,  // HEX: 0x0000 0020 - DEC: 32
        Arcane = 0x40,  // HEX: 0x0000 0040 - DEC: 64
    }
}
