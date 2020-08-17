using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObjects.Structs;
using AmeisenBotX.Core.Data.Objects.WowObjects.Structs.SubStructs;
using System;
using System.Collections.Generic;
using System.Text;

namespace AmeisenBotX.Core.Data.Objects.WowObjects
{
    public class WowPlayer : WowUnit
    {
        public WowPlayer(IntPtr baseAddress, WowObjectType type, IntPtr descriptorAddress) : base(baseAddress, type, descriptorAddress)
        {
        }

        public int ComboPoints { get; set; }

        public bool IsFlying { get; set; }

        public bool IsSwimming { get; set; }

        public bool IsUnderwater { get; set; }

        public List<VisibleItemEnchantment> ItemEnchantments { get; private set; }

        public int NextLevelXp { get; set; }

        public List<QuestlogEntry> QuestlogEntries { get; private set; }

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

        public override string ToString()
        {
            return $"Player: [{Guid}] {Name} lvl. {Level}";
        }

        public unsafe override void Update()
        {
            base.Update();

            if (WowInterface.I.XMemory.ReadStruct(DescriptorAddress + RawWowObject.EndOffset + RawWowUnit.EndOffset, out RawWowPlayer objPtr))
            {
                Xp = objPtr.Xp;
                NextLevelXp = objPtr.NextLevelXp;

                XpPercentage = BotMath.Percentage(Xp, NextLevelXp);

                Name = ReadPlayerName();

                QuestlogEntries = new List<QuestlogEntry>()
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

                ItemEnchantments = new List<VisibleItemEnchantment>()
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
        }

        private string ReadPlayerName()
        {
            if (WowInterface.I.BotCache.TryGetUnitName(Guid, out string cachedName))
            {
                return cachedName;
            }

            uint shortGuid;
            uint offset;

            WowInterface.I.XMemory.Read(IntPtr.Add(WowInterface.I.OffsetList.NameStore, (int)WowInterface.I.OffsetList.NameMask), out uint nameMask);
            WowInterface.I.XMemory.Read(IntPtr.Add(WowInterface.I.OffsetList.NameStore, (int)WowInterface.I.OffsetList.NameBase), out uint nameBase);

            shortGuid = (uint)Guid & 0xfffffff;
            offset = 12 * (nameMask & shortGuid);

            WowInterface.I.XMemory.Read(new IntPtr(nameBase + offset + 8), out uint current);
            WowInterface.I.XMemory.Read(new IntPtr(nameBase + offset), out offset);

            if ((current & 0x1) == 0x1)
            {
                return string.Empty;
            }

            WowInterface.I.XMemory.Read(new IntPtr(current), out uint testGuid);

            while (testGuid != shortGuid)
            {
                WowInterface.I.XMemory.Read(new IntPtr(current + offset + 4), out current);

                if ((current & 0x1) == 0x1)
                {
                    return string.Empty;
                }

                WowInterface.I.XMemory.Read(new IntPtr(current), out testGuid);
            }

            WowInterface.I.XMemory.ReadString(new IntPtr(current + (int)WowInterface.I.OffsetList.NameString), Encoding.UTF8, out string name, 16);

            if (name.Length > 0)
            {
                WowInterface.I.BotCache.CacheName(Guid, name);
            }

            return name;
        }
    }
}