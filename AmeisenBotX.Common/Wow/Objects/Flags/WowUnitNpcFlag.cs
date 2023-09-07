using System;

namespace AmeisenBotX.Wow.Objects.Flags
{
    [Flags]
    public enum WowUnitNpcFlag
    {
        None = 0x0,
        Gossip = 0x1,
        Questgiver = 0x2,
        Trainer = 0x10,
        ClassTrainer = 0x20,
        ProfessionTrainer = 0x40,
        Vendor = 0x80,
        AmmoVendor = 0x100,
        FoodVendor = 0x200,
        PoisonVendor = 0x400,
        ReagentVendor = 0x800,
        Repairer = 0x1000,
        FlightMaster = 0x2000,
        SpiritHealer = 0x4000,
        SpiritGuide = 0x8000,
        Innkeeper = 0x10000,
        Banker = 0x20000,
        Petitioner = 0x40000,
        TabardDesigner = 0x80000,
        Battlemaster = 0x100000,
        Auctioneer = 0x200000,
        StableMaster = 0x400000,
        GuildBanker = 0x800000,
        Spellclick = 0x1000000,
        Mailbox = 0x4000000,
        ForgeMaster = 0x8000000,
        Guard = 0x10000000,
        Transmogrifier = 0x10000000,
        VoidStorageBanker = 0x20000000,
    }
}