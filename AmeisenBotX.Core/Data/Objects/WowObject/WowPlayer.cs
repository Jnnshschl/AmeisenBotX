using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject.Structs;
using AmeisenBotX.Core.Data.Objects.WowObject.Structs.SubStructs;
using System;
using System.Collections.Generic;
using System.Text;

namespace AmeisenBotX.Core.Data.Objects.WowObject
{
    public class WowPlayer : WowUnit
    {
        public WowPlayer(IntPtr baseAddress, WowObjectType type, IntPtr descriptorAddress) : base(baseAddress, type, descriptorAddress)
        {
        }

        public int ComboPoints { get; set; }

        public List<VisibleItemEnchantment> ItemEnchantments { get; private set; }

        public List<QuestlogEntry> QuestlogEntries { get; private set; }

        public bool IsFlying { get; set; }

        public bool IsSwimming { get; set; }

        public bool IsUnderwater { get; set; }

        public int NextLevelXp { get; set; }

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

            Name = string.Empty;

            fixed (RawWowPlayer* objPtr = stackalloc RawWowPlayer[1])
            {
                if (WowInterface.I.XMemory.ReadStruct(DescriptorAddress + RawWowObject.EndOffset + RawWowPlayer.EndOffset, objPtr))
                {
                    Xp = objPtr[0].Xp;
                    NextLevelXp = objPtr[0].NextLevelXp;

                    XpPercentage = BotMath.Percentage(Xp, NextLevelXp);

                    Name = ReadPlayerName();

                    QuestlogEntries = new List<QuestlogEntry>()
                    {
                        objPtr[0].QuestlogEntry1,
                        objPtr[0].QuestlogEntry2,
                        objPtr[0].QuestlogEntry3,
                        objPtr[0].QuestlogEntry4,
                        objPtr[0].QuestlogEntry5,
                        objPtr[0].QuestlogEntry6,
                        objPtr[0].QuestlogEntry7,
                        objPtr[0].QuestlogEntry8,
                        objPtr[0].QuestlogEntry9,
                        objPtr[0].QuestlogEntry10,
                        objPtr[0].QuestlogEntry11,
                        objPtr[0].QuestlogEntry12,
                        objPtr[0].QuestlogEntry13,
                        objPtr[0].QuestlogEntry14,
                        objPtr[0].QuestlogEntry15,
                        objPtr[0].QuestlogEntry16,
                        objPtr[0].QuestlogEntry17,
                        objPtr[0].QuestlogEntry18,
                        objPtr[0].QuestlogEntry19,
                        objPtr[0].QuestlogEntry20,
                        objPtr[0].QuestlogEntry21,
                        objPtr[0].QuestlogEntry22,
                        objPtr[0].QuestlogEntry23,
                        objPtr[0].QuestlogEntry24,
                        objPtr[0].QuestlogEntry25,
                    };

                    ItemEnchantments = new List<VisibleItemEnchantment>()
                    {
                        objPtr[0].VisibleItemEnchantment1,
                        objPtr[0].VisibleItemEnchantment2,
                        objPtr[0].VisibleItemEnchantment3,
                        objPtr[0].VisibleItemEnchantment4,
                        objPtr[0].VisibleItemEnchantment5,
                        objPtr[0].VisibleItemEnchantment6,
                        objPtr[0].VisibleItemEnchantment7,
                        objPtr[0].VisibleItemEnchantment8,
                        objPtr[0].VisibleItemEnchantment9,
                        objPtr[0].VisibleItemEnchantment10,
                        objPtr[0].VisibleItemEnchantment11,
                        objPtr[0].VisibleItemEnchantment12,
                        objPtr[0].VisibleItemEnchantment13,
                        objPtr[0].VisibleItemEnchantment14,
                        objPtr[0].VisibleItemEnchantment15,
                        objPtr[0].VisibleItemEnchantment16,
                        objPtr[0].VisibleItemEnchantment17,
                        objPtr[0].VisibleItemEnchantment18,
                        objPtr[0].VisibleItemEnchantment19,
                    };
                }
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