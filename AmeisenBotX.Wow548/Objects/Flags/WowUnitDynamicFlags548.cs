namespace AmeisenBotX.Wow548.Objects.Flags
{
    [Flags]
    public enum WowUnitDynamicFlags548
    {
        None = 0,
        Invisible = 0x1,
        Lootable = 0x2,
        TrackUnit = 0x4,
        TaggedByOther = 0x8,
        TaggedByMe = 0x10,
        SpecialInfo = 0x20,
        Dead = 0x40,
        ReferAFriendLinked = 0x80,
        IsTappedByAllThreatList = 0x100
    }
}