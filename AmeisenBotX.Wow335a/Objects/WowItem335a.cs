using AmeisenBotX.Memory;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Raw.Enums;
using AmeisenBotX.Wow.Objects.Raw.SubStructs;
using AmeisenBotX.Wow.Offsets;
using AmeisenBotX.Wow335a.Objects.Descriptors;
using System;
using System.Collections.Generic;

namespace AmeisenBotX.Wow335a.Objects
{
    [Serializable]
    public class WowItem335a : WowObject335a, IWowItem
    {
        public int Count { get; set; }

        public List<ItemEnchantment> ItemEnchantments { get; private set; }

        public ulong Owner { get; set; }

        public IEnumerable<string> GetEnchantmentStrings()
        {
            List<string> enchantments = new();

            foreach (ItemEnchantment itemEnch in ItemEnchantments)
                if (WowEnchantmentHelper.TryLookupEnchantment(itemEnch.Id, out string text))
                    enchantments.Add(text);

            return enchantments;
        }

        public override string ToString()
        {
            return $"Item: [{Guid}] ({EntryId}) Owner: {Owner} Count: {Count}";
        }

        public override void Update(IMemoryApi memoryApi, IOffsetList offsetList)
        {
            base.Update(memoryApi, offsetList);

            if (memoryApi.Read(DescriptorAddress + WowObjectDescriptor335a.EndOffset, out WowItemDescriptor335a objPtr))
            {
                Count = objPtr.StackCount;
                Owner = objPtr.Owner;

                ItemEnchantments = new List<ItemEnchantment>
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