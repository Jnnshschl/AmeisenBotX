using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Offsets;
using AmeisenBotX.Memory;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Wow.Objects.SubStructs;
using System;
using System.Collections.Generic;
using System.Text;

namespace AmeisenBotX.Core.Data.Objects
{
    public class WowPlayer : WowUnit
    {
        private VisibleItemEnchantment[] itemEnchantments;
        private QuestlogEntry[] questlogEntries;

        public WowPlayer(IntPtr baseAddress, WowObjectType type, IntPtr descriptorAddress) : base(baseAddress, type, descriptorAddress)
        {
        }

        public int ComboPoints { get; set; }

        public bool IsFlying { get; set; }

        public bool IsGhost { get; set; }

        public bool IsOutdoors { get; set; }

        public bool IsSwimming { get; set; }

        public bool IsUnderwater { get; set; }

        public IEnumerable<VisibleItemEnchantment> ItemEnchantments => itemEnchantments;

        public int NextLevelXp { get; set; }

        public IEnumerable<QuestlogEntry> QuestlogEntries => questlogEntries;

        public int Xp { get; set; }

        public double XpPercentage { get; set; }

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

        public override string ReadName(IMemoryApi memoryApi, IOffsetList offsetList)
        {
            memoryApi.Read(IntPtr.Add(offsetList.NameStore, (int)offsetList.NameMask), out uint nameMask);
            memoryApi.Read(IntPtr.Add(offsetList.NameStore, (int)offsetList.NameBase), out uint nameBase);

            uint shortGuid = (uint)Guid & 0xfffffff;
            uint offset = 12 * (nameMask & shortGuid);

            memoryApi.Read(new(nameBase + offset + 8), out uint current);
            memoryApi.Read(new(nameBase + offset), out offset);

            if ((current & 0x1) == 0x1)
            {
                return string.Empty;
            }

            memoryApi.Read(new(current), out uint testGuid);

            while (testGuid != shortGuid)
            {
                memoryApi.Read(new(current + offset + 4), out current);

                if ((current & 0x1) == 0x1)
                {
                    return string.Empty;
                }

                memoryApi.Read(new(current), out testGuid);
            }

            memoryApi.ReadString(new(current + (int)offsetList.NameString), Encoding.UTF8, out string name, 16);

            return name;
        }

        public override string ToString()
        {
            return $"Player: {Guid} lvl. {Level}";
        }

        public override void Update(IMemoryApi memoryApi, IOffsetList offsetList)
        {
            base.Update(memoryApi, offsetList);

            if (memoryApi.Read(DescriptorAddress + RawWowObject.EndOffset + RawWowUnit.EndOffset, out RawWowPlayer objPtr))
            {
                Xp = objPtr.Xp;
                NextLevelXp = objPtr.NextLevelXp;
                XpPercentage = BotMath.Percentage(Xp, NextLevelXp);
                // Name = ReadPlayerName(memoryApi, offsetList);

                questlogEntries = new QuestlogEntry[]
                {
                    objPtr.QuestlogEntry1,
                    objPtr.QuestlogEntry2,
                    objPtr.QuestlogEntry3,
                    objPtr.QuestlogEntry4,
                    objPtr.QuestlogEntry5,
                    objPtr.QuestlogEntry6,
                    objPtr.QuestlogEntry7,
                    objPtr.QuestlogEntry8,
                    objPtr.QuestlogEntry9,
                    objPtr.QuestlogEntry10,
                    objPtr.QuestlogEntry11,
                    objPtr.QuestlogEntry12,
                    objPtr.QuestlogEntry13,
                    objPtr.QuestlogEntry14,
                    objPtr.QuestlogEntry15,
                    objPtr.QuestlogEntry16,
                    objPtr.QuestlogEntry17,
                    objPtr.QuestlogEntry18,
                    objPtr.QuestlogEntry19,
                    objPtr.QuestlogEntry20,
                    objPtr.QuestlogEntry21,
                    objPtr.QuestlogEntry22,
                    objPtr.QuestlogEntry23,
                    objPtr.QuestlogEntry24,
                    objPtr.QuestlogEntry25,
                };

                itemEnchantments = new VisibleItemEnchantment[]
                {
                    objPtr.VisibleItemEnchantment1,
                    objPtr.VisibleItemEnchantment2,
                    objPtr.VisibleItemEnchantment3,
                    objPtr.VisibleItemEnchantment4,
                    objPtr.VisibleItemEnchantment5,
                    objPtr.VisibleItemEnchantment6,
                    objPtr.VisibleItemEnchantment7,
                    objPtr.VisibleItemEnchantment8,
                    objPtr.VisibleItemEnchantment9,
                    objPtr.VisibleItemEnchantment10,
                    objPtr.VisibleItemEnchantment11,
                    objPtr.VisibleItemEnchantment12,
                    objPtr.VisibleItemEnchantment13,
                    objPtr.VisibleItemEnchantment14,
                    objPtr.VisibleItemEnchantment15,
                    objPtr.VisibleItemEnchantment16,
                    objPtr.VisibleItemEnchantment17,
                    objPtr.VisibleItemEnchantment18,
                    objPtr.VisibleItemEnchantment19,
                };
            }

            if (memoryApi.Read(IntPtr.Add(BaseAddress, (int)offsetList.WowUnitSwimFlags), out uint swimFlags))
            {
                IsSwimming = (swimFlags & 0x200000) != 0;
            }

            if (memoryApi.Read(IntPtr.Add(BaseAddress, (int)offsetList.WowUnitFlyFlagsPointer), out IntPtr flyFlagsPointer)
                && memoryApi.Read(IntPtr.Add(flyFlagsPointer, (int)offsetList.WowUnitFlyFlags), out uint flyFlags))
            {
                IsFlying = (flyFlags & 0x2000000) != 0;
            }

            if (memoryApi.Read(offsetList.BreathTimer, out int breathTimer))
            {
                IsUnderwater = breathTimer > 0;
            }

            IsGhost = HasBuffById(8326);
        }
    }
}