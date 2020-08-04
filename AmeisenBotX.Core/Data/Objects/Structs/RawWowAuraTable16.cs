using System.Runtime.InteropServices;

namespace AmeisenBotX.Core.Data.Objects.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RawWowAuraTable16 : IRawWowAuraTable
    {
        public int MaxBuffs => 16;

        public RawWowAura Aura1 { get; set; }

        public RawWowAura Aura2 { get; set; }

        public RawWowAura Aura3 { get; set; }

        public RawWowAura Aura4 { get; set; }

        public RawWowAura Aura5 { get; set; }

        public RawWowAura Aura6 { get; set; }

        public RawWowAura Aura7 { get; set; }

        public RawWowAura Aura8 { get; set; }

        public RawWowAura Aura9 { get; set; }

        public RawWowAura Aura10 { get; set; }

        public RawWowAura Aura11 { get; set; }

        public RawWowAura Aura12 { get; set; }

        public RawWowAura Aura13 { get; set; }

        public RawWowAura Aura14 { get; set; }

        public RawWowAura Aura15 { get; set; }

        public RawWowAura Aura16 { get; set; }

        public RawWowAura[] AsArray()
        {
            return new RawWowAura[]
            {
                Aura1,
                Aura2,
                Aura3,
                Aura4,
                Aura5,
                Aura6,
                Aura7,
                Aura8,
                Aura9,
                Aura10,
                Aura11,
                Aura12,
                Aura13,
                Aura14,
                Aura15,
                Aura16,
            };
        }

        public WowAura[] AsAuraArray(WowInterface wowInterface)
        {
            return new WowAura[]
            {
                new WowAura(wowInterface, Aura1),
                new WowAura(wowInterface, Aura2),
                new WowAura(wowInterface, Aura3),
                new WowAura(wowInterface, Aura4),
                new WowAura(wowInterface, Aura5),
                new WowAura(wowInterface, Aura6),
                new WowAura(wowInterface, Aura7),
                new WowAura(wowInterface, Aura8),
                new WowAura(wowInterface, Aura9),
                new WowAura(wowInterface, Aura10),
                new WowAura(wowInterface, Aura11),
                new WowAura(wowInterface, Aura12),
                new WowAura(wowInterface, Aura13),
                new WowAura(wowInterface, Aura14),
                new WowAura(wowInterface, Aura15),
                new WowAura(wowInterface, Aura16),
            };
        }
    }
}