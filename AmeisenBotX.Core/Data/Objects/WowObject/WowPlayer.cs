using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject.Structs;
using AmeisenBotX.Memory;

namespace AmeisenBotX.Core.Data.Objects.WowObject
{
    public class WowPlayer : WowUnit
    {
        public override int BaseOffset => RawWowObject.EndOffset + RawWowUnit.EndOffset;

        public int ComboPoints { get; set; }

        public int Exp { get; set; }

        public int MaxExp { get; set; }

        private RawWowPlayer RawWowPlayer { get; set; }

        public bool IsAlliance()
            => Race == WowRace.Draenei
            || Race == WowRace.Human
            || Race == WowRace.Dwarf
            || Race == WowRace.Gnome
            || Race == WowRace.Nightelf;

        public bool IsHorde()
            => Race == WowRace.Undead
            || Race == WowRace.Orc
            || Race == WowRace.Bloodelf
            || Race == WowRace.Tauren
            || Race == WowRace.Undead;

        public override WowObject Update(XMemory xMemory)
        {
            if (xMemory.ReadStruct(BaseAddress, out RawWowPlayer rawWowPlayer))
            {
                RawWowPlayer = rawWowPlayer;
            }

            return this;
        }
    }
}