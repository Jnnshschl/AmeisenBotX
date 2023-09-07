namespace AmeisenBotX.Wow.Objects
{
    public interface IWowAura
    {
        public ulong Creator { get; }

        public byte Flags { get; }

        public byte Level { get; }

        public int SpellId { get; }

        public byte StackCount { get; }
    }
}