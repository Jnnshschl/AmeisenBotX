using AmeisenBotX.Core.Data.Objects.WowObject.Structs;
using AmeisenBotX.Memory;
using System;

namespace AmeisenBotX.Core.Data.Objects.WowObject
{
    [Serializable]
    public class WowContainer : WowObject
    {
        public WowContainer(IntPtr baseAddress, WowObjectType type) : base(baseAddress, type)
        {
        }

        public int SlotCount { get; set; }

        public override string ToString()
        {
            return $"Container: [{Guid}] SlotCount: {SlotCount}";
        }

        public WowContainer UpdateRawWowContainer()
        {
            UpdateRawWowObject();

            if (WowInterface.I.XMemory.ReadStruct(DescriptorAddress + RawWowObject.EndOffset, out RawWowContainer rawWowContainer))
            {
                SlotCount = rawWowContainer.SlotCount;
            }

            return this;
        }
    }
}