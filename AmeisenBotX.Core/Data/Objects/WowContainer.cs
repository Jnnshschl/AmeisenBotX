using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.Raw;
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

        public override void Update(WowInterface wowInterface)
        {
            base.Update(wowInterface);

            if (wowInterface.XMemory.Read(DescriptorAddress + RawWowObject.EndOffset, out RawWowContainer objPtr))
            {
                SlotCount = objPtr.SlotCount;
            }
        }
    }
}