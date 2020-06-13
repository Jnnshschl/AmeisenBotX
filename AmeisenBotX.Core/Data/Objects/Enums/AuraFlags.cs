using System;

namespace AmeisenBotX.Core.Data.Objects.Enums
{
    [Flags]
    public enum AuraFlags
    {
        Passive = 0x10,
        Harmful = 0x20,
        Active = 0x80
    }
}