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
    public unsafe class WowPlayer548 : WowUnit548, IWowPlayer
    {
        protected uint? MovementFlags;

        protected WowPlayerDescriptor548? PlayerDescriptor;

        private IEnumerable<VisibleItemEnchantment> itemEnchantments;

        private IEnumerable<QuestlogEntry> questlogEntries;

        public int ComboPoints => Memory.Read(Offsets.ComboPoints, out byte comboPoints) ? comboPoints : 0;

        public bool IsFlying => (GetMovementFlags() & 0x1000000) != 0;

        public bool IsGhost => HasBuffById(8326);

        public bool IsOutdoors { get; set; }

        public bool IsSwimming => (GetMovementFlags() & 0x200000) != 0;

        public bool IsUnderwater { get; set; }

        public IEnumerable<VisibleItemEnchantment> ItemEnchantments => itemEnchantments;

        public int NextLevelXp => GetPlayerDescriptor().NextLevelXp;

        public IEnumerable<QuestlogEntry> QuestlogEntries => questlogEntries;

        public int Xp => GetPlayerDescriptor().Xp;

        public double XpPercentage => BotMath.Percentage(Xp, NextLevelXp);

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

            // if (memoryApi.Read(offsetList.BreathTimer, out int breathTimer)) { IsUnderwater =
            // breathTimer > 0; }
        }

        protected uint GetMovementFlags() => MovementFlags ??= Memory.Read(IntPtr.Add(BaseAddress, (int)Offsets.WowUnitSwimFlags), out IntPtr movementFlagsPtr) 
            && Memory.Read(IntPtr.Add(movementFlagsPtr, 0x38), out uint movementFlags) ? movementFlags : 0;

        protected WowPlayerDescriptor548 GetPlayerDescriptor() => PlayerDescriptor ??= Memory.Read(DescriptorAddress + sizeof(WowObjectDescriptor548) + sizeof(WowUnitDescriptor548), out WowPlayerDescriptor548 objPtr) ? objPtr : new();
    }
}