using System;

namespace AmeisenBotX.Core.Data.Objects.WowObject
{
    [Flags]
    public enum WowUnitNpcFlags : int
    {
        None = 0x00000000,
        Gossip = 0x00000001,
        Questgiver = 0x00000002,
        Trainer = 0x00000010,
        ClassTrainer = 0x00000020,
        ProfessionTrainer = 0x00000040,
        Vendr = 0x00000080,
        GeneralGoodVendor = 0x00000100,
        FoodVendor = 0x00000200,
        PoisonVendor = 0x00000400,
        ReagentVendor = 0x00000800,
        RepairVendor = 0x00001000,
        Flightmaster = 0x00002000,
        Spirithealer = 0x00004000,
        Spiritguide = 0x00008000,
        Innkeeper = 0x00010000,
        Banker = 0x00020000,
        Petitioner = 0x00040000,
        Tabarddesigner = 0x00080000,
        Battlemaster = 0x00100000,
        Auctioneer = 0x00200000,
        Stablemaster = 0x00400000,
        Guildbanker = 0x00800000,
        Spellclick = 0x01000000,
        Guard = 0x10000000,
    }
}
