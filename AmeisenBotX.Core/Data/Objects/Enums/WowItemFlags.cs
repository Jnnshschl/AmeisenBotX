using System;

namespace AmeisenBotX.Core.Data.Objects.Enums
{
    [Flags]
    public enum WowItemFlags : int
    {
        ItemFlagNone = 0x0,
        ItemFlagSoulbound = 0x1,
        ItemFlagConjured = 0x2,
        ItemFlagLootable = 0x4,
        ItemFlagWrapGift = 0x200,
        ItemFlagCreateItem = 0x400,
        ItemFlagQuest = 0x800,
        ItemFlagRefundable = 0x1000,
        ItemFlagSignable = 0x2000,
        ItemFlagReadable = 0x4000,
        ItemFlagEventReq = 0x10000,
        ItemFlagProspectable = 0x40000,
        ItemFlagUniqueEquip = 0x80000,
        ItemFlagThrown = 0x400000,
        ItemFlagShapeshiftOK = 0x800000,
        ItemFlagAccountBound = 0x8000000,
        ItemFlagMillable = 0x20000000
    }
}