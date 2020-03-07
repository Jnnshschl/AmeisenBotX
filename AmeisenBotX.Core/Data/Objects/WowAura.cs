using AmeisenBotX.Core.Data.Objects.Structs;

namespace AmeisenBotX.Core.Data.Objects
{
    public class WowAura
    {
        public WowAura(RawWowAura rawWowAura, string name)
        {
            RawWowAura = rawWowAura;
            Name = name;
        }

        public ulong Creator => RawWowAura.Creator;

        public string Name { get; private set; }

        private RawWowAura RawWowAura { get; set; }
    }
}