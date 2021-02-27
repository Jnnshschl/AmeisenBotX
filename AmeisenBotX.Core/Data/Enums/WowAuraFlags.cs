using System;

namespace AmeisenBotX.Core.Data.Enums
{
    [Flags]
    public enum WowAuraFlags
    {
        Passive = 0x10,
        Harmful = 0x20,
        Active = 0x80
    }
}