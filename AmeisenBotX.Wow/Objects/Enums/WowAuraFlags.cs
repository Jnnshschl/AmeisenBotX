﻿using System;

namespace AmeisenBotX.Wow.Objects.Enums
{
    [Flags]
    public enum WowAuraFlags
    {
        Passive = 0x10,
        Harmful = 0x20,
        Active = 0x80
    }
}