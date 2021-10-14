using System;

namespace AmeisenBotX.Wow.Objects.Flags
{
    [Flags]
    public enum WowAuraFlag
    {
        Passive = 0x10,
        Harmful = 0x20,
        Active = 0x80
    }
}