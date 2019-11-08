using System;

namespace AmeisenBotX.Core.Character.Spells.Enums
{
    [Flags]
    public enum Spellschools
    {
        Physical = 1 << 0,
        Holy = 1 << 1,
        Fire = 1 << 2,
        Nature = 1 << 3,
        Frost = 1 << 4,
        Shadow = 1 << 5,
        Arcane = 1 << 6
    }
}
