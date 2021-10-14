using System;

namespace AmeisenBotX.Wow.Objects.Flags
{
    [Flags]
    public enum WowReputationFlag : short
    {
        None = 0x0,                       // HEX: 0x0000 - DEC: 0
        Visible = 0x1,                    // HEX: 0x0001 - DEC: 1
        AtWar = 0x2,                      // HEX: 0x0002 - DEC: 2
        Hidden = 0x4,                     // HEX: 0x0004 - DEC: 4
        Header = 0x8,                     // HEX: 0x0008 - DEC: 8
        Peaceful = 0x10,                  // HEX: 0x0010 - DEC: 16
        Inactive = 0x20,                  // HEX: 0x0020 - DEC: 32
        ShowPropagated = 0x40,            // HEX: 0x0040 - DEC: 64
        HeaderShowsBar = 0x80,            // HEX: 0x0080 - DEC: 128
        CapitalCityForRaceChange = 0x100, // HEX: 0x0100 - DEC: 256
        Guild = 0x200,                    // HEX: 0x0200 - DEC: 512
        GarrisonInvasion = 0x400          // HEX: 0x0400 - DEC: 1024
    }
}
