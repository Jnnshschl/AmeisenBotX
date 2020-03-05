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

        private RawWowItem RawWowItem { get; set; }

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