using System;

namespace AmeisenBotX.Wow.Objects.Enums
{
    [Flags]
    public enum WowDispelTypes
    {
        None = 0,
        Curse = 1 << 0,
        Disease = 1 << 1,
        Magic = 1 << 2,
        Poison = 1 << 3
    }
}