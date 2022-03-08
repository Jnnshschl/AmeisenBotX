using AmeisenBotX.Common.Math;
using AmeisenBotX.Memory;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Wow.Objects.Raw.SubStructs;
using AmeisenBotX.Wow.Offsets;
using AmeisenBotX.Wow548.Objects.Descriptors;
using System.Text;

namespace AmeisenBotX.Wow548.Objects
{
    [Serializable]
    public class WowPlayer548 : WowUnit548, IWowPlayer
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

        protected WowPlayerDescriptor548 RawWowPlayer { get; private set; }

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

        public override string ReadName(IMemoryApi memoryApi, IOffsetList offsetList)
        {
            if (memoryApi.Read(IntPtr.Add(offsetList.NameStore, (int)offsetList.NameMask), out uint nameMask)
                && memoryApi.Read(IntPtr.Add(offsetList.NameStore, (int)offsetList.NameBase), out uint nameBase))
            {
                uint shortGuid = (uint)Guid & 0xfffffff;
                uint offset = 12 * (nameMask & shortGuid);

                if (memoryApi.Read(new(nameBase + offset + 8), out uint current)
                    && memoryApi.Read(new(nameBase + offset), out offset))
                {
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

                    if (memoryApi.ReadString(new(current + (int)offsetList.NameString), Encoding.UTF8, out string name, 16))
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

        public override void Update(IMemoryApi memoryApi, IOffsetList offsetList)
        {
            base.Update(memoryApi, offsetList);

            if (memoryApi.Read(DescriptorAddress + WowObjectDescriptor548.EndOffset + WowUnitDescriptor548.EndOffset, out WowPlayerDescriptor548 obj))
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

            if (memoryApi.Read(IntPtr.Add(BaseAddress, (int)offsetList.WowUnitSwimFlags), out uint swimFlags))
            {
                IsSwimming = (swimFlags & 0x200000) != 0;
            }

            if (memoryApi.Read(IntPtr.Add(BaseAddress, (int)offsetList.WowUnitFlyFlagsPointer), out IntPtr flyFlagsPointer)
                && memoryApi.Read(IntPtr.Add(flyFlagsPointer, (int)offsetList.WowUnitFlyFlags), out uint flyFlags))
            {
                IsFlying = (flyFlags & 0x2000000) != 0;
            }

            // if (memoryApi.Read(offsetList.BreathTimer, out int breathTimer))
            // {
            //     IsUnderwater = breathTimer > 0;
            // }

            if (memoryApi.Read(offsetList.ComboPoints, out byte comboPoints))
            {
                ComboPoints = comboPoints;
            }

            IsGhost = HasBuffById(8326);
        }
    }
}