using AmeisenBotX.Core.Data.Objects.WowObject.Structs;
using AmeisenBotX.Core.Data.Objects.WowObject.Structs.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject.Structs.SubStructs;
using System;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Data.Objects.WowObject
{
    public class WowItem : WowObject
    {
        public WowItem(IntPtr baseAddress, WowObjectType type, IntPtr descriptorAddress) : base(baseAddress, type, descriptorAddress)
        {
        }

        public int Count { get; set; }

        public List<ItemEnchantment> ItemEnchantments { get; private set; }

        public ulong Owner { get; set; }

        public List<string> GetEnchantmentStrings()
        {
            List<string> enchantments = new List<string>();

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

        public unsafe override void Update()
        {
            base.Update();

            fixed (RawWowItem* objPtr = stackalloc RawWowItem[1])
            {
                if (WowInterface.I.XMemory.ReadStruct(DescriptorAddress + RawWowObject.EndOffset, objPtr))
                {
                    Count = objPtr[0].StackCount;
                    Owner = objPtr[0].Owner;

                    ItemEnchantments = new List<ItemEnchantment>()
                        {
                            objPtr[0].Enchantment1,
                            objPtr[0].Enchantment2,
                            objPtr[0].Enchantment3,
                            objPtr[0].Enchantment4,
                            objPtr[0].Enchantment5,
                            objPtr[0].Enchantment6,
                            objPtr[0].Enchantment7,
                            objPtr[0].Enchantment8,
                            objPtr[0].Enchantment9,
                            objPtr[0].Enchantment10,
                            objPtr[0].Enchantment11,
                            objPtr[0].Enchantment12,
                        };
                }
            }
        }
    }
}