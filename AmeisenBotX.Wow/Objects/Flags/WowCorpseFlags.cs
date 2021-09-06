using System;

namespace AmeisenBotX.Wow.Objects.Flags
{
    [Flags]
    public enum WowCorpseFlags
    {
        None = 0x0,
        Bones = 0x1,
        Unknown1 = 0x2,
        PvP = 0x4,
        HideHelmet = 0x8,
        HideCloak = 0x10,
        Skinnable = 0x20,
        FFAPvP = 0x40
    }
}