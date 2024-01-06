using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Raw.Enums;
using AmeisenBotX.Wow.Objects.Raw.SubStructs;
using AmeisenBotX.Wow548.Objects.Descriptors;

namespace AmeisenBotX.Wow548.Objects
{
    [Serializable]
    public unsafe class WowItem548 : WowObject548, IWowItem
    {
        protected WowItemDescriptor548? ItemDescriptor;

        public int Count => GetItemDescriptor().StackCount;

        public List<ItemEnchantment> ItemEnchantments { get; private set; }

        public ulong Owner => GetItemDescriptor().Owner;

        public IEnumerable<string> GetEnchantmentStrings()
        {
            List<string> enchantments = [];

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

        public override void Update()
        {
            base.Update();

            // ItemEnchantments = new() { objPtr.Enchantment1, objPtr.Enchantment2,
            // objPtr.Enchantment3, objPtr.Enchantment4, objPtr.Enchantment5, objPtr.Enchantment6,
            // objPtr.Enchantment7, objPtr.Enchantment8, objPtr.Enchantment9, objPtr.Enchantment10,
            // objPtr.Enchantment11, objPtr.Enchantment12, };
        }

        protected WowItemDescriptor548 GetItemDescriptor()
        {
            return ItemDescriptor ??= Memory.Read(DescriptorAddress + sizeof(WowObjectDescriptor548), out WowItemDescriptor548 objPtr) ? objPtr : new();
        }
    }
}