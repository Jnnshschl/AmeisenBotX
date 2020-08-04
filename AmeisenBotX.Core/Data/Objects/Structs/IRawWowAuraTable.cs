namespace AmeisenBotX.Core.Data.Objects.Structs
{
    public interface IRawWowAuraTable
    {
        int MaxBuffs { get; }

        RawWowAura[] AsArray();

        WowAura[] AsAuraArray(WowInterface wowInterface);
    }
}