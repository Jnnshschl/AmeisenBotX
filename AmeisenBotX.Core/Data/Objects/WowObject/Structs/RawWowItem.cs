using System.Runtime.InteropServices;

namespace AmeisenBotX.Core.Data.Objects.WowObject.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct RawWowItem
    {
        public ulong Owner;
        public ulong Contained;
        public ulong Creator;
        public ulong GiftCreator;
        public int StackCount;
        public int Duration;
        public fixed int SpellCharges[5];
        public int Flags;
        public fixed int Enchantment1_1[2];
        public fixed short Enchantment1_3[2];
        public fixed int Enchantment2_1[2];
        public fixed short Enchantment2_3[2];
        public fixed int Enchantment3_1[2];
        public fixed short Enchantment3_3[2];
        public fixed int Enchantment4_1[2];
        public fixed short Enchantment4_3[2];
        public fixed int Enchantment5_1[2];
        public fixed short Enchantment5_3[2];
        public fixed int Enchantment6_1[2];
        public fixed short Enchantment6_3[2];
        public fixed int Enchantment7_1[2];
        public fixed short Enchantment7_3[2];
        public fixed int Enchantment8_1[2];
        public fixed short Enchantment8_3[2];
        public fixed int Enchantment9_1[2];
        public fixed short Enchantment9_3[2];
        public fixed int Enchantment10_1[2];
        public fixed short Enchantment10_3[2];
        public fixed int Enchantment11_1[2];
        public fixed short Enchantment11_3[2];
        public fixed int Enchantment12_1[2];
        public fixed short Enchantment12_3[2];
        public int PropertySeed;
        public int RandomPropertiesId;
        public int Durability;
        public int MaxDurability;
        public int CreatePlayedTime;
        public int WowItemPad;

        public const int EndOffset = 232;
    }
}