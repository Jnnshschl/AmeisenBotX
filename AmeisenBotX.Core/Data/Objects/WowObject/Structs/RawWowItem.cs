using AmeisenBotX.Core.Data.Objects.WowObject.Structs.SubStructs;
using System;
using System.Runtime.InteropServices;

namespace AmeisenBotX.Core.Data.Objects.WowObject.Structs
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct RawWowItem
    {
        public ulong Owner;
        public ulong Contained;
        public ulong Creator;
        public ulong GiftCreator;
        public int StackCount;
        public int Duration;
        public int SpellCharge1;
        public int SpellCharge2;
        public int SpellCharge3;
        public int SpellCharge4;
        public int SpellCharge5;
        public int Flags;
        public ItemEnchantment Enchantment1;
        public ItemEnchantment Enchantment2;
        public ItemEnchantment Enchantment3;
        public ItemEnchantment Enchantment4;
        public ItemEnchantment Enchantment5;
        public ItemEnchantment Enchantment6;
        public ItemEnchantment Enchantment7;
        public ItemEnchantment Enchantment8;
        public ItemEnchantment Enchantment9;
        public ItemEnchantment Enchantment10;
        public ItemEnchantment Enchantment11;
        public ItemEnchantment Enchantment12;
        public int PropertySeed;
        public int RandomPropertiesId;
        public int Durability;
        public int MaxDurability;
        public int CreatePlayedTime;
        public int WowItemPad;

        public const int EndOffset = 232;
    }
}