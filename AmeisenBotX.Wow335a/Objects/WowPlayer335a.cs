using AmeisenBotX.Common.Math;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Wow.Objects.Raw.SubStructs;
using AmeisenBotX.Wow335a.Objects.Descriptors;
using System;
using System.Collections.Generic;
using System.Text;

namespace AmeisenBotX.Wow335a.Objects
{
    [Serializable]
    public class WowPlayer335a : WowUnit335a, IWowPlayer
    {
        private VisibleItemEnchantment[] itemEnchantments;

        private QuestlogEntry[] questlogEntries;

        public int ComboPoints { get; set; }

        public bool IsFlying { get; set; }

        public bool IsGhost { get; set; }

        public bool IsOutdoors { get; set; }

        public bool IsSwimming { get; set; }

        public bool IsUnderwater { get; set; }

        public IEnumerable<VisibleItemEnchantment> ItemEnchantments => itemEnchantments;

        public int NextLevelXp => RawWowPlayer.NextLevelXp;

        public IEnumerable<QuestlogEntry> QuestlogEntries => questlogEntries;

        public int Xp => RawWowPlayer.Xp;

        public double XpPercentage => BotMath.Percentage(Xp, NextLevelXp);

        protected WowPlayerDescriptor335a RawWowPlayer { get; private set; }

        public bool IsAlliance()
        {
            return Race is WowRace.Draenei
                or WowRace.Human
                or WowRace.Dwarf
                or WowRace.Gnome
                or WowRace.Nightelf;
        }

        public bool IsHorde()
        {
            return Race is WowRace.Undead
                or WowRace.Orc
                or WowRace.Bloodelf
                or WowRace.Tauren
                or WowRace.Troll;
        }

        public override string ReadName()
        {
            if (Memory.Read(IntPtr.Add(Memory.Offsets.NameStore, (int)Memory.Offsets.NameMask), out uint nameMask)
                && Memory.Read(IntPtr.Add(Memory.Offsets.NameStore, (int)Memory.Offsets.NameBase), out uint nameBase))
            {
                uint shortGuid = (uint)Guid & 0xfffffff;
                uint offset = 12 * (nameMask & shortGuid);

                if (Memory.Read(new(nameBase + offset + 8), out uint current)
                    && Memory.Read(new(nameBase + offset), out offset))
                {
                    if ((current & 0x1) == 0x1)
                    {
                        return string.Empty;
                    }

                    Memory.Read(new(current), out uint testGuid);

                    while (testGuid != shortGuid)
                    {
                        Memory.Read(new(current + offset + 4), out current);

                        if ((current & 0x1) == 0x1)
                        {
                            return string.Empty;
                        }

                        Memory.Read(new(current), out testGuid);
                    }

                    if (Memory.ReadString(new(current + (int)Memory.Offsets.NameString), Encoding.UTF8, out string name, 16))
                    {
                        return name;
                    }
                }
            }

            return string.Empty;
        }

        public override string ToString()
        {
            return $"Player: {Guid} lvl. {Level}";
        }

        public override void Update()
        {
            base.Update();

            if (Memory.Read(DescriptorAddress + WowObjectDescriptor335a.EndOffset + WowUnitDescriptor335a.EndOffset, out WowPlayerDescriptor335a obj))
            {
                RawWowPlayer = obj;

                questlogEntries = new QuestlogEntry[]
                {
                    obj.QuestlogEntry1,
                    obj.QuestlogEntry2,
                    obj.QuestlogEntry3,
                    obj.QuestlogEntry4,
                    obj.QuestlogEntry5,
                    obj.QuestlogEntry6,
                    obj.QuestlogEntry7,
                    obj.QuestlogEntry8,
                    obj.QuestlogEntry9,
                    obj.QuestlogEntry10,
                    obj.QuestlogEntry11,
                    obj.QuestlogEntry12,
                    obj.QuestlogEntry13,
                    obj.QuestlogEntry14,
                    obj.QuestlogEntry15,
                    obj.QuestlogEntry16,
                    obj.QuestlogEntry17,
                    obj.QuestlogEntry18,
                    obj.QuestlogEntry19,
                    obj.QuestlogEntry20,
                    obj.QuestlogEntry21,
                    obj.QuestlogEntry22,
                    obj.QuestlogEntry23,
                    obj.QuestlogEntry24,
                    obj.QuestlogEntry25,
                };

                itemEnchantments = new VisibleItemEnchantment[]
                {
                    obj.VisibleItemEnchantment1,
                    obj.VisibleItemEnchantment2,
                    obj.VisibleItemEnchantment3,
                    obj.VisibleItemEnchantment4,
                    obj.VisibleItemEnchantment5,
                    obj.VisibleItemEnchantment6,
                    obj.VisibleItemEnchantment7,
                    obj.VisibleItemEnchantment8,
                    obj.VisibleItemEnchantment9,
                    obj.VisibleItemEnchantment10,
                    obj.VisibleItemEnchantment11,
                    obj.VisibleItemEnchantment12,
                    obj.VisibleItemEnchantment13,
                    obj.VisibleItemEnchantment14,
                    obj.VisibleItemEnchantment15,
                    obj.VisibleItemEnchantment16,
                    obj.VisibleItemEnchantment17,
                    obj.VisibleItemEnchantment18,
                    obj.VisibleItemEnchantment19,
                };
            }

            if (Memory.Read(IntPtr.Add(BaseAddress, 0xA30), out uint swimFlags))
            {
                IsSwimming = (swimFlags & 0x200000) != 0;
            }

            if (Memory.Read(IntPtr.Add(BaseAddress, 0xD8), out IntPtr flyFlagsPointer)
                && Memory.Read(IntPtr.Add(flyFlagsPointer, 0x44), out uint flyFlags))
            {
                IsFlying = (flyFlags & 0x2000000) != 0;
            }

            if (Memory.Read(Memory.Offsets.BreathTimer, out int breathTimer))
            {
                IsUnderwater = breathTimer > 0;
            }

            if (Memory.Read(Memory.Offsets.ComboPoints, out byte comboPoints))
            {
                ComboPoints = comboPoints;
            }

            IsGhost = HasBuffById(8326);
        }
    }
}