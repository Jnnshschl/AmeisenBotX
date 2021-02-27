using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.Raw;
using AmeisenBotX.Core.Data.Objects.Raw.SubStructs;
using AmeisenBotX.Core.Movement.Enums;
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

        public void Interact(WowInterface wowInterface, WowGameobject gameobject, float minRange = 3.0f)
        {
            if (IsInRange(gameobject, minRange))
            {
                wowInterface.HookManager.WowObjectRightClick(gameobject);
            }
            else
            {
                wowInterface.MovementEngine.SetMovementAction(MovementAction.Move, gameobject.Position);
            }
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

        public override unsafe void Update(WowInterface wowInterface)
        {
            base.Update(wowInterface);

            if (wowInterface.XMemory.ReadStruct(DescriptorAddress + RawWowObject.EndOffset + RawWowUnit.EndOffset, out RawWowPlayer objPtr))
            {
                Xp = objPtr.Xp;
                NextLevelXp = objPtr.NextLevelXp;
                XpPercentage = BotMath.Percentage(Xp, NextLevelXp);
                Name = ReadPlayerName(wowInterface);

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

            if (wowInterface.XMemory.Read(IntPtr.Add(BaseAddress, (int)wowInterface.OffsetList.WowUnitSwimFlags), out uint swimFlags))
            {
                IsSwimming = (swimFlags & 0x200000) != 0;
            }

            if (wowInterface.XMemory.Read(IntPtr.Add(BaseAddress, (int)wowInterface.OffsetList.WowUnitFlyFlagsPointer), out IntPtr flyFlagsPointer)
                && wowInterface.XMemory.Read(IntPtr.Add(flyFlagsPointer, (int)wowInterface.OffsetList.WowUnitFlyFlags), out uint flyFlags))
            {
                IsFlying = (flyFlags & 0x2000000) != 0;
            }

            if (wowInterface.XMemory.Read(wowInterface.OffsetList.BreathTimer, out int breathTimer))
            {
                IsUnderwater = breathTimer > 0;
            }

            IsGhost = HasBuffById(8326);
        }

        private string ReadPlayerName(WowInterface wowInterface)
        {
            if (wowInterface.Db.TryGetUnitName(Guid, out string cachedName))
            {
                return cachedName;
            }

            wowInterface.XMemory.Read(IntPtr.Add(wowInterface.OffsetList.NameStore, (int)wowInterface.OffsetList.NameMask), out uint nameMask);
            wowInterface.XMemory.Read(IntPtr.Add(wowInterface.OffsetList.NameStore, (int)wowInterface.OffsetList.NameBase), out uint nameBase);

            uint shortGuid = (uint)Guid & 0xfffffff;
            uint offset = 12 * (nameMask & shortGuid);

            wowInterface.XMemory.Read(new IntPtr(nameBase + offset + 8), out uint current);
            wowInterface.XMemory.Read(new IntPtr(nameBase + offset), out offset);

            if ((current & 0x1) == 0x1)
            {
                return string.Empty;
            }

            wowInterface.XMemory.Read(new IntPtr(current), out uint testGuid);

            while (testGuid != shortGuid)
            {
                wowInterface.XMemory.Read(new IntPtr(current + offset + 4), out current);

                if ((current & 0x1) == 0x1)
                {
                    return string.Empty;
                }

                wowInterface.XMemory.Read(new IntPtr(current), out testGuid);
            }

            wowInterface.XMemory.ReadString(new IntPtr(current + (int)wowInterface.OffsetList.NameString), Encoding.UTF8, out string name, 16);

            if (name.Length > 0)
            {
                wowInterface.Db.CacheName(Guid, name);
            }

            return name;
        }
    }
}