﻿using System;

namespace AmeisenBotX.Wow.Objects.Enums
{
    [Flags]
    public enum WowUnitDynamicFlags : int
    {
        None = 0x0,
        Lootable = 0x1,
        TrackUnit = 0x2,
        TaggedByOther = 0x4,
        TaggedByMe = 0x8,
        SpecialInfo = 0x10,
        Dead = 0x20,
        ReferAFriendLinked = 0x40,
        TappedByThreat = 0x80,
    }
}