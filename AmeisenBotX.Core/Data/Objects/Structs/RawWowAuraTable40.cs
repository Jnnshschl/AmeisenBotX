using System.Runtime.InteropServices;

namespace AmeisenBotX.Core.Data.Objects.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RawWowAuraTable40 : IRawWowAuraTable
    {
        public int MaxBuffs => 40;

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

        public RawWowAura Aura17 { get; set; }

        public RawWowAura Aura18 { get; set; }

        public RawWowAura Aura19 { get; set; }

        public RawWowAura Aura20 { get; set; }

        public RawWowAura Aura21 { get; set; }

        public RawWowAura Aura22 { get; set; }

        public RawWowAura Aura23 { get; set; }

        public RawWowAura Aura24 { get; set; }

        public RawWowAura Aura25 { get; set; }

        public RawWowAura Aura26 { get; set; }

        public RawWowAura Aura27 { get; set; }

        public RawWowAura Aura28 { get; set; }

        public RawWowAura Aura29 { get; set; }

        public RawWowAura Aura30 { get; set; }

        public RawWowAura Aura31 { get; set; }

        public RawWowAura Aura32 { get; set; }

        public RawWowAura Aura33 { get; set; }

        public RawWowAura Aura34 { get; set; }

        public RawWowAura Aura35 { get; set; }

        public RawWowAura Aura36 { get; set; }

        public RawWowAura Aura37 { get; set; }

        public RawWowAura Aura38 { get; set; }

        public RawWowAura Aura39 { get; set; }

        public RawWowAura Aura40 { get; set; }

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
                Aura17,
                Aura18,
                Aura19,
                Aura20,
                Aura21,
                Aura22,
                Aura23,
                Aura24,
                Aura25,
                Aura26,
                Aura27,
                Aura28,
                Aura29,
                Aura30,
                Aura31,
                Aura32,
                Aura33,
                Aura34,
                Aura35,
                Aura36,
                Aura37,
                Aura38,
                Aura39,
                Aura40,
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
                new WowAura(wowInterface, Aura17),
                new WowAura(wowInterface, Aura18),
                new WowAura(wowInterface, Aura19),
                new WowAura(wowInterface, Aura20),
                new WowAura(wowInterface, Aura21),
                new WowAura(wowInterface, Aura22),
                new WowAura(wowInterface, Aura23),
                new WowAura(wowInterface, Aura24),
                new WowAura(wowInterface, Aura25),
                new WowAura(wowInterface, Aura26),
                new WowAura(wowInterface, Aura27),
                new WowAura(wowInterface, Aura28),
                new WowAura(wowInterface, Aura29),
                new WowAura(wowInterface, Aura30),
                new WowAura(wowInterface, Aura31),
                new WowAura(wowInterface, Aura32),
                new WowAura(wowInterface, Aura33),
                new WowAura(wowInterface, Aura34),
                new WowAura(wowInterface, Aura35),
                new WowAura(wowInterface, Aura36),
                new WowAura(wowInterface, Aura37),
                new WowAura(wowInterface, Aura38),
                new WowAura(wowInterface, Aura39),
                new WowAura(wowInterface, Aura40),
            };
        }
    }
}