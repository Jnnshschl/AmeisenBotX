using System;

namespace AmeisenBotX.Core.Data.Objects.WowObjects
{
    [Flags]
    public enum WowGameobjectFlags : int
    {
        InUse = 0x1,
        Locked = 0x2,
        ConditionalInteraction = 0x4,
        Transport = 0x8,
        DoesNotDespawn = 0x20,
        Triggered = 0x40,
    }
}