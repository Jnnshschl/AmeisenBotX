using AmeisenBotX.Core.Data.Objects.WowObjects.Structs;
using System;

namespace AmeisenBotX.Core.Data.Objects.WowObjects
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

        public override unsafe void Update()
        {
            base.Update();

            if (WowInterface.I.XMemory.ReadStruct(DescriptorAddress + RawWowObject.EndOffset, out RawWowContainer objPtr))
            {
                SlotCount = objPtr.SlotCount;
            }
        }
    }
}