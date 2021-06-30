using AmeisenBotX.Common.Offsets;
using AmeisenBotX.Memory;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System;

namespace AmeisenBotX.Core.Data.Objects
{
    [Serializable]
    public class WowContainer : WowObject
    {
        public WowContainer(IntPtr baseAddress, WowObjectType type, IntPtr descriptorAddress) : base(baseAddress, type, descriptorAddress)
        {
        }

        public int SlotCount { get; set; }

        public override string ToString()
        {
            return $"Container: [{Guid}] SlotCount: {SlotCount}";
        }

        public override void Update(XMemory xMemory, IOffsetList offsetList)
        {
            base.Update(xMemory, offsetList);

            if (xMemory.Read(DescriptorAddress + RawWowObject.EndOffset, out RawWowContainer objPtr))
            {
                SlotCount = objPtr.SlotCount;
            }
        }
    }
}