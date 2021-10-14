using System;

namespace AmeisenBotX.Wow.Objects.Flags
{
    [Flags]
    public enum WowItemFlag : uint
    {
        None = 0x0,
        NoPickup = 0x1,
        Conjured = 0x2,
        HasLoot = 0x4,
        HeroicTooltip = 0x8,
        Deprecated = 0x10,
        NoUserDestroy = 0x20,
        PlayerCast = 0x40,
        NoEquipCooldown = 0x80,
        MultiLootQuest = 0x100,
        IsWrapper = 0x200,
        UsesResources = 0x400,
        MultiDrop = 0x800,
        ItemPurchaseRecord = 0x1000,
        Petition = 0x2000,
        HasText = 0x4000,
        NoDisenchant = 0x8000,
        RealDuration = 0x10000,
        NoCreator = 0x20000,
        IsProspectable = 0x40000,
        UniqueEquipable = 0x80000,
        IgnoreForAuras = 0x100000,
        IgnoreDefaultArenaRestrictions = 0x200000,
        NoDurabilityLoss = 0x400000,
        UseWhenShapeshifted = 0x800000,
        HasQuestGlow = 0x1000000,
        HidenUsableRecipe = 0x2000000,
        NotUsableInArena = 0x4000000,
        BoundToAccount = 0x8000000,
        NoReagentCost = 0x10000000,
        IsMillable = 0x20000000,
        ReportToGuildChat = 0x40000000,
        NoProgressiveLoot = 0x80000000
    }
}