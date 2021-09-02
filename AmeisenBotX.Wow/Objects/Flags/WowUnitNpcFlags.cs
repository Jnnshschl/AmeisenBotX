using System;

namespace AmeisenBotX.Wow.Objects.Flags
{
    [Flags]
    public enum WowUnitNpcFlags : int
    {
        None = 0x0,               // HEX: 0x0000 0000 - DEC: 0
        Gossip = 0x1,             // HEX: 0x0000 0001 - DEC: 1
        Questgiver = 0x2,         // HEX: 0x0000 0002 - DEC: 2
        Trainer = 0x10,           // HEX: 0x0000 0010 - DEC: 16
        ClassTrainer = 0x20,      // HEX: 0x0000 0020 - DEC: 32
        ProfessionTrainer = 0x40, // HEX: 0x0000 0040 - DEC: 64
        Vendor = 0x80,            // HEX: 0x0000 0080 - DEC: 128
        AmmoVendor = 0x100,       // HEX: 0x0000 0100 - DEC: 256
        FoodVendor = 0x200,       // HEX: 0x0000 0200 - DEC: 512
        PoisonVendor = 0x400,     // HEX: 0x0000 0400 - DEC: 1024
        ReagentVendor = 0x800,    // HEX: 0x0000 0800 - DEC: 2048
        Repairer = 0x1000,        // HEX: 0x0000 1000 - DEC: 4096; Note: NPC with this flag can have nothing to sell/buy, so technically speaking not a vendor at all.
        FlightMaster = 0x2000,    // HEX: 0x0000 2000 - DEC: 8192
        SpiritHealer = 0x4000,    // HEX: 0x0000 4000 - DEC: 16384
        SpiritGuide = 0x8000,     // HEX: 0x0000 8000 - DEC: 32768
        Innkeeper = 0x10000,      // HEX: 0x0001 0000 - DEC: 65536
        Banker = 0x20000,         // HEX: 0x0002 0000 - DEC: 131072
        Petitioner = 0x40000,     // HEX: 0x0004 0000 - DEC: 262144
        TabardDesigner = 0x80000, // HEX: 0x0008 0000 - DEC: 524288
        Battlemaster = 0x100000,  // HEX: 0x0010 0000 - DEC: 1048576
        Auctioneer = 0x200000,    // HEX: 0x0020 0000 - DEC: 2097152
        StableMaster = 0x400000,  // HEX: 0x0040 0000 - DEC: 4194304
        GuildBanker = 0x800000,   // HEX: 0x0080 0000 - DEC: 8388608
        Spellclick = 0x1000000,   // HEX: 0x0100 0000 - DEC: 16777216
        Mailbox = 0x4000000,      // HEX: 0x0400 0000 - DEC: 67108864; Note: NPC will upon right-click behave like mailbox
        Guard = 0x10000000,       // HEX: 0x1000 0000 - DEC: 268435456
    }
}