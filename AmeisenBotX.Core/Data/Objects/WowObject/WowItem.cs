using AmeisenBotX.Core.Data.Objects.WowObject.Structs;
using AmeisenBotX.Core.Data.Objects.WowObject.Structs.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject.Structs.SubStructs;
using AmeisenBotX.Memory;
using System;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Data.Objects.WowObject
{
    [Serializable]
    public class WowItem : WowObject
    {
        public WowItem(IntPtr baseAddress, WowObjectType type) : base(baseAddress, type)
        {
        }

        public int Count => RawWowItem.StackCount;

        public ulong Owner => RawWowItem.Owner;

        private RawWowItem RawWowItem { get; set; }

        public List<string> GetEnchantmentStrings()
        {
            List<string> enchantments = new List<string>();

            List<ItemEnchantment> itemEnchants = GetItemEnchantments();

            for (int i = 0; i < itemEnchants.Count; ++i)
            {
                if (WowEnchantmentHelper.TryLookupEnchantment(itemEnchants[i].Id, out string text))
                {
                    enchantments.Add(text);
                }
            }

            return enchantments;
        }

        public List<ItemEnchantment> GetItemEnchantments()
        {
            return new List<ItemEnchantment>()
            {
                RawWowItem.Enchantment1,
                RawWowItem.Enchantment2,
                RawWowItem.Enchantment3,
                RawWowItem.Enchantment4,
                RawWowItem.Enchantment5,
                RawWowItem.Enchantment6,
                RawWowItem.Enchantment7,
                RawWowItem.Enchantment8,
                RawWowItem.Enchantment9,
                RawWowItem.Enchantment10,
                RawWowItem.Enchantment11,
                RawWowItem.Enchantment12,
            };
        }

        public override string ToString()
        {
            return $"Item: [{Guid}] ({EntryId}) Owner: {Owner} Count: {Count}";
        }

        public WowItem UpdateRawWowItem(XMemory xMemory)
        {
            UpdateRawWowObject(xMemory);

            if (xMemory.ReadStruct(DescriptorAddress + RawWowObject.EndOffset, out RawWowItem rawWowItem))
            {
                RawWowItem = rawWowItem;
            }

            return this;
        }
    }
}