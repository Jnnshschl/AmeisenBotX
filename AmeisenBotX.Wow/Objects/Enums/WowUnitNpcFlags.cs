using System;

namespace AmeisenBotX.Wow.Objects.Enums
{
    [Flags]
    public enum WowUnitNpcFlags : int
    {
        None = 0x0,
        Gossip = 0x1,
        Questgiver = 0x2,
        Trainer = 0x10,
        ClassTrainer = 0x20,
        ProfessionTrainer = 0x40,
        Vendor = 0x80,
        GeneralGoodsVendor = 0x100,
        FoodVendor = 0x200,
        PoisonVendor = 0x400,
        ReagentVendor = 0x800,
        RepairVendor = 0x1000,
        Flightmaster = 0x2000,
        Spirithealer = 0x4000,
        Spiritguide = 0x8000,
        Innkeeper = 0x10000,
        Banker = 0x20000,
        Petitioner = 0x40000,
        Tabarddesigner = 0x80000,
        Battlemaster = 0x100000,
        Auctioneer = 0x200000,
        Stablemaster = 0x400000,
        Guildbanker = 0x800000,
        Spellclick = 0x1000000,
        Guard = 0x10000000,
    }
}