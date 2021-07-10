using AmeisenBotX.Common.Offsets;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Memory;
using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Wow.Objects.SubStructs;
using AmeisenBotX.Wow335a.Objects.Descriptors;
using System;
using System.Collections.Generic;

namespace AmeisenBotX.Wow335a.Objects
{
    [Serializable]
    public class WowItem335a : WowObject335a, IWowItem
    {
        public WowItem335a(IntPtr baseAddress, IntPtr descriptorAddress) : base(baseAddress, descriptorAddress)
        {
        }

        public int Count { get; set; }

        public List<ItemEnchantment> ItemEnchantments { get; private set; }

        public ulong Owner { get; set; }

        public IEnumerable<string> GetEnchantmentStrings()
        {
            List<string> enchantments = new();

            for (int i = 0; i < ItemEnchantments.Count; ++i)
            {
                if (WowEnchantmentHelper.TryLookupEnchantment(ItemEnchantments[i].Id, out string text))
                {
                    enchantments.Add(text);
                }
            }

            return enchantments;
        }

        public override string ToString()
        {
            return $"Item: [{Guid}] ({EntryId}) Owner: {Owner} Count: {Count}";
        }

        public override void Update(IMemoryApi memoryApi, IOffsetList offsetList)
        {
            base.Update(memoryApi, offsetList);

            if (memoryApi.Read(DescriptorAddress + WowObjectDescriptor.EndOffset, out WowItemDescriptor objPtr))
            {
                Count = objPtr.StackCount;
                Owner = objPtr.Owner;

                ItemEnchantments = new()
                {
                    objPtr.Enchantment1,
                    objPtr.Enchantment2,
                    objPtr.Enchantment3,
                    objPtr.Enchantment4,
                    objPtr.Enchantment5,
                    objPtr.Enchantment6,
                    objPtr.Enchantment7,
                    objPtr.Enchantment8,
                    objPtr.Enchantment9,
                    objPtr.Enchantment10,
                    objPtr.Enchantment11,
                    objPtr.Enchantment12,
                };
            }
        }
    }
}