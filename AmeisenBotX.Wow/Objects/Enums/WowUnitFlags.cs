using System;

namespace AmeisenBotX.Wow.Objects.Enums
{
    [Flags]
    public enum WowUnitFlags : int
    {
        None = 0x0,
        Sitting = 0x1,
        Influenced = 0x4,
        PlayerControlled = 0x8,
        Totem = 0x10,
        Preparation = 0x20,
        PlusMob = 0x40,
        NotAttackable = 0x100,
        Looting = 0x400,
        PetInCombat = 0x800,
        PvPFlagged = 0x1000,
        Silenced = 0x2000,
        Pacified = 0x20000,
        Stunned = 0x40000,
        CanPerformAction = 0x60000,
        Combat = 0x80000,
        TaxiFlight = 0x100000,
        Disarmed = 0x200000,
        Confused = 0x400000,
        Fleeing = 0x800000,
        Possessed = 0x1000000,
        NotSelectable = 0x2000000,
        Skinnable = 0x4000000,
        Mounted = 0x8000000,
        Dazed = 0x20000000,
        Sheathe = 0x40000000,
    }
}