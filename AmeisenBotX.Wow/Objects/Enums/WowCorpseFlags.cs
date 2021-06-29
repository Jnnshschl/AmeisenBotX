﻿using System;

namespace AmeisenBotX.Wow.Objects.Enums
{
    [Flags]
    public enum WowCorpseFlags : int
    {
        None = 0x0,
        Bones = 0x1,
        HideHelmet = 0x8,
        HideCloak = 0x10,
        Lootable = 0x20
    }
}