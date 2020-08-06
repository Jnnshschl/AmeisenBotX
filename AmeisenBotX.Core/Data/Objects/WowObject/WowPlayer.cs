using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject.Structs;
using AmeisenBotX.Core.Data.Objects.WowObject.Structs.SubStructs;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Memory;
using System;
using System.Collections.Generic;
using System.Dynamic;

namespace AmeisenBotX.Core.Data.Objects.WowObject
{
    public class WowPlayer : WowUnit
    {
        public WowPlayer(IntPtr baseAddress, WowObjectType type) : base(baseAddress, type)
        {
        }

        public int ComboPoints { get; set; }

        public bool IsFlying { get; set; }

        public bool IsSwimming { get; set; }

        public bool IsUnderwater { get; set; }

        public int NextLevelXp { get; set; }

        public int Xp { get; set; }

        public double XpPercentage => ReturnPercentage(Xp, NextLevelXp);

        public List<VisibleItemEnchantment> GetItemEnchantments { get; set; }

        public List<QuestlogEntry> GetQuestlogEntries { get; set; }

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

        public WowPlayer UpdateRawWowPlayer()
        {
            UpdateRawWowUnit();

            if (WowInterface.I.XMemory.ReadStruct(DescriptorAddress + RawWowObject.EndOffset + RawWowUnit.EndOffset, out RawWowPlayer rawWowPlayer))
            {
                Xp = rawWowPlayer.Xp;
                NextLevelXp = rawWowPlayer.NextLevelXp;

                GetQuestlogEntries = new List<QuestlogEntry>()
                {
                    rawWowPlayer.QuestlogEntry1,
                    rawWowPlayer.QuestlogEntry2,
                    rawWowPlayer.QuestlogEntry3,
                    rawWowPlayer.QuestlogEntry4,
                    rawWowPlayer.QuestlogEntry5,
                    rawWowPlayer.QuestlogEntry6,
                    rawWowPlayer.QuestlogEntry7,
                    rawWowPlayer.QuestlogEntry8,
                    rawWowPlayer.QuestlogEntry9,
                    rawWowPlayer.QuestlogEntry10,
                    rawWowPlayer.QuestlogEntry11,
                    rawWowPlayer.QuestlogEntry12,
                    rawWowPlayer.QuestlogEntry13,
                    rawWowPlayer.QuestlogEntry14,
                    rawWowPlayer.QuestlogEntry15,
                    rawWowPlayer.QuestlogEntry16,
                    rawWowPlayer.QuestlogEntry17,
                    rawWowPlayer.QuestlogEntry18,
                    rawWowPlayer.QuestlogEntry19,
                    rawWowPlayer.QuestlogEntry20,
                    rawWowPlayer.QuestlogEntry21,
                    rawWowPlayer.QuestlogEntry22,
                    rawWowPlayer.QuestlogEntry23,
                    rawWowPlayer.QuestlogEntry24,
                    rawWowPlayer.QuestlogEntry25,
                };

                GetItemEnchantments = new List<VisibleItemEnchantment>()
                {
                    rawWowPlayer.VisibleItemEnchantment1,
                    rawWowPlayer.VisibleItemEnchantment2,
                    rawWowPlayer.VisibleItemEnchantment3,
                    rawWowPlayer.VisibleItemEnchantment4,
                    rawWowPlayer.VisibleItemEnchantment5,
                    rawWowPlayer.VisibleItemEnchantment6,
                    rawWowPlayer.VisibleItemEnchantment7,
                    rawWowPlayer.VisibleItemEnchantment8,
                    rawWowPlayer.VisibleItemEnchantment9,
                    rawWowPlayer.VisibleItemEnchantment10,
                    rawWowPlayer.VisibleItemEnchantment11,
                    rawWowPlayer.VisibleItemEnchantment12,
                    rawWowPlayer.VisibleItemEnchantment13,
                    rawWowPlayer.VisibleItemEnchantment14,
                    rawWowPlayer.VisibleItemEnchantment15,
                    rawWowPlayer.VisibleItemEnchantment16,
                    rawWowPlayer.VisibleItemEnchantment17,
                    rawWowPlayer.VisibleItemEnchantment18,
                    rawWowPlayer.VisibleItemEnchantment19,
                };
            }

            if (WowInterface.I.XMemory.Read(IntPtr.Add(BaseAddress, (int)WowInterface.I.OffsetList.WowUnitSwimFlags), out uint swimFlags))
            {
                IsSwimming = (swimFlags & 0x200000) != 0;
            }

            if (WowInterface.I.XMemory.Read(IntPtr.Add(BaseAddress, (int)WowInterface.I.OffsetList.WowUnitFlyFlagsPointer), out IntPtr flyFlagsPointer)
            && WowInterface.I.XMemory.Read(IntPtr.Add(flyFlagsPointer, (int)WowInterface.I.OffsetList.WowUnitFlyFlags), out uint flyFlags))
            {
                IsFlying = (flyFlags & 0x2000000) != 0;
            }

            if (WowInterface.I.XMemory.Read(WowInterface.I.OffsetList.BreathTimer, out int breathTimer))
            {
                IsUnderwater = breathTimer > 0;
            }

            return this;
        }
    }
}