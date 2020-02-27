using System;

namespace AmeisenBotX.Core.Data.Objects.WowObject
{
    [Flags]
    public enum WowGameobjectFlags : int
    {
        InUse = 0x01,
        Locked = 0x02,
        ConditionalInteraction = 0x04,
        Transport = 0x08,
        DoesNotDespawn = 0x20,
        Triggered = 0x40,
    }
}