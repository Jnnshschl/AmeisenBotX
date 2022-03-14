using AmeisenBotX.Common.Math;
using AmeisenBotX.Wow;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Wow.Objects.Raw.SubStructs;
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

        public int ComboPoints => Memory.Read(Memory.Offsets.ComboPoints, out byte comboPoints) ? comboPoints : 0;

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

        public override string ReadName(WowMemoryApi memory)
        {
            if (memory.Read(IntPtr.Add(memory.Offsets.NameStore, (int)memory.Offsets.NameMask), out uint nameMask)
                && memory.Read(IntPtr.Add(memory.Offsets.NameStore, (int)memory.Offsets.NameBase), out uint nameBase))
            {
                uint shortGuid = (uint)Guid & 0xfffffff;
                uint offset = 12 * (nameMask & shortGuid);

                if (memory.Read(new(nameBase + offset + 8), out uint current)
                    && memory.Read(new(nameBase + offset), out offset))
                {
                    if ((current & 0x1) == 0x1)
                    {
                        return string.Empty;
                    }

                    memory.Read(new(current), out uint testGuid);

                    while (testGuid != shortGuid)
                    {
                        memory.Read(new(current + offset + 4), out current);

                        if ((current & 0x1) == 0x1)
                        {
                            return string.Empty;
                        }

                        memory.Read(new(current), out testGuid);
                    }

                    if (memory.ReadString(new(current + (int)memory.Offsets.NameString), Encoding.UTF8, out string name, 16))
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

        public override void Update(WowMemoryApi memory)
        {
            base.Update(memory);

            // if (memory.Read(offsetList.BreathTimer, out int breathTimer)) { IsUnderwater =
            // breathTimer > 0; }
        }

        protected uint GetMovementFlags()
        {
            return MovementFlags ??= Memory.Read(IntPtr.Add(BaseAddress, (int)Memory.Offsets.WowUnitSwimFlags), out IntPtr movementFlagsPtr)
&& Memory.Read(IntPtr.Add(movementFlagsPtr, 0x38), out uint movementFlags) ? movementFlags : 0;
        }

        protected WowPlayerDescriptor548 GetPlayerDescriptor()
        {
            return PlayerDescriptor ??= Memory.Read(DescriptorAddress + sizeof(WowObjectDescriptor548) + sizeof(WowUnitDescriptor548), out WowPlayerDescriptor548 objPtr) ? objPtr : new();
        }
    }
}