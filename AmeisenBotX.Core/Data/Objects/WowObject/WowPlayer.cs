using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject.Structs;
using AmeisenBotX.Core.Data.Objects.WowObject.Structs.SubStructs;
using AmeisenBotX.Memory;
using System;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Data.Objects.WowObject
{
    [Serializable]
    public class WowPlayer : WowUnit
    {
        public WowPlayer(IntPtr baseAddress, WowObjectType type) : base(baseAddress, type)
        {
        }

        public int ComboPoints { get; set; }

        public bool IsFlying { get; set; }

        public bool IsSwimming { get; set; }

        public bool IsUnderwater { get; set; }

        public int NextLevelXp => RawWowPlayer.NextLevelXp;

        public int Xp => RawWowPlayer.Xp;

        public double XpPercentage => ReturnPercentage(Xp, NextLevelXp);

        private RawWowPlayer RawWowPlayer { get; set; }

        public List<VisibleItemEnchantment> GetItemEnchantments()
        {
            return new List<VisibleItemEnchantment>()
            {
                RawWowPlayer.VisibleItemEnchantment1,
                RawWowPlayer.VisibleItemEnchantment2,
                RawWowPlayer.VisibleItemEnchantment3,
                RawWowPlayer.VisibleItemEnchantment4,
                RawWowPlayer.VisibleItemEnchantment5,
                RawWowPlayer.VisibleItemEnchantment6,
                RawWowPlayer.VisibleItemEnchantment7,
                RawWowPlayer.VisibleItemEnchantment8,
                RawWowPlayer.VisibleItemEnchantment9,
                RawWowPlayer.VisibleItemEnchantment10,
                RawWowPlayer.VisibleItemEnchantment11,
                RawWowPlayer.VisibleItemEnchantment12,
                RawWowPlayer.VisibleItemEnchantment13,
                RawWowPlayer.VisibleItemEnchantment14,
                RawWowPlayer.VisibleItemEnchantment15,
                RawWowPlayer.VisibleItemEnchantment16,
                RawWowPlayer.VisibleItemEnchantment17,
                RawWowPlayer.VisibleItemEnchantment18,
                RawWowPlayer.VisibleItemEnchantment19,
            };
        }

        public List<QuestlogEntry> GetQuestlogEntries()
        {
            return new List<QuestlogEntry>()
            {
                RawWowPlayer.QuestlogEntry1,
                RawWowPlayer.QuestlogEntry2,
                RawWowPlayer.QuestlogEntry3,
                RawWowPlayer.QuestlogEntry4,
                RawWowPlayer.QuestlogEntry5,
                RawWowPlayer.QuestlogEntry6,
                RawWowPlayer.QuestlogEntry7,
                RawWowPlayer.QuestlogEntry8,
                RawWowPlayer.QuestlogEntry9,
                RawWowPlayer.QuestlogEntry10,
                RawWowPlayer.QuestlogEntry11,
                RawWowPlayer.QuestlogEntry12,
                RawWowPlayer.QuestlogEntry13,
                RawWowPlayer.QuestlogEntry14,
                RawWowPlayer.QuestlogEntry15,
                RawWowPlayer.QuestlogEntry16,
                RawWowPlayer.QuestlogEntry17,
                RawWowPlayer.QuestlogEntry18,
                RawWowPlayer.QuestlogEntry19,
                RawWowPlayer.QuestlogEntry20,
                RawWowPlayer.QuestlogEntry21,
                RawWowPlayer.QuestlogEntry22,
                RawWowPlayer.QuestlogEntry23,
                RawWowPlayer.QuestlogEntry24,
                RawWowPlayer.QuestlogEntry25,
            };
        }

        public bool IsAlliance()
        {
            return Race == WowRace.Draenei
                        || Race == WowRace.Human
                        || Race == WowRace.Dwarf
                        || Race == WowRace.Gnome
                        || Race == WowRace.Nightelf;
        }

        public bool IsHorde()
        {
            return Race == WowRace.Undead
                       || Race == WowRace.Orc
                       || Race == WowRace.Bloodelf
                       || Race == WowRace.Tauren
                       || Race == WowRace.Troll;
        }

        public override string ToString()
        {
            return $"Player: [{Guid}] {Name} lvl. {Level}";
        }

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