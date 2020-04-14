using AmeisenBotX.Core.Data.Objects.WowObject.Structs;
using AmeisenBotX.Memory;
using System;

namespace AmeisenBotX.Core.Data.Objects.WowObject
{
    public class WowItem : WowObject
    {
        public WowItem(IntPtr baseAddress, WowObjectType type) : base(baseAddress, type)
        {
        }

        public int Count => RawWowItem.StackCount;

        public ulong Owner => RawWowItem.Owner;

        private RawWowItem RawWowItem { get; set; }

        public override string ToString()
            => $"Item: [{Guid}] Owner: {Owner} Count: {Count}";

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