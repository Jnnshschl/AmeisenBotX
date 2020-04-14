using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject.Structs;
using AmeisenBotX.Memory;
using System;

namespace AmeisenBotX.Core.Data.Objects.WowObject
{
    public class WowPlayer : WowUnit
    {
        public WowPlayer(IntPtr baseAddress, WowObjectType type) : base(baseAddress, type)
        {
        }

        public int ComboPoints { get; set; }

        public int NextLevelXp => RawWowPlayer.NextLevelXp;

        public int Xp => RawWowPlayer.Xp;

        public double XpPercentage => ReturnPercentage(Xp, NextLevelXp);

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

        public override string ToString()
                            => $"Player: [{Guid}] {Name} lvl. {Level}";

        public WowPlayer UpdateRawWowPlayer(XMemory xMemory)
        {
            UpdateRawWowUnit(xMemory);

            if (xMemory.ReadStruct(DescriptorAddress + RawWowObject.EndOffset + RawWowUnit.EndOffset, out RawWowPlayer rawWowPlayer))
            {
                RawWowPlayer = rawWowPlayer;
            }

            return this;
        }
    }
}