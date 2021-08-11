using AmeisenBotX.Memory;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Wow.Offsets;
using AmeisenBotX.Wow335a.Objects.Descriptors;
using System;

namespace AmeisenBotX.Wow335a.Objects
{
    [Serializable]
    public class WowContainer335a : WowObject335a, IWowContainer
    {
        public WowContainer335a(IntPtr baseAddress, IntPtr descriptorAddress) : base(baseAddress, descriptorAddress)
        {
            Type = WowObjectType.Container;
        }

        public int SlotCount => RawWowContainer.SlotCount;

        protected WowContainerDescriptor RawWowContainer { get; private set; }

        public override string ToString()
        {
            return $"Container: [{Guid}] SlotCount: {SlotCount}";
        }

        public override void Update(IMemoryApi memoryApi, IOffsetList offsetList)
        {
            base.Update(memoryApi, offsetList);

            if (memoryApi.Read(DescriptorAddress + WowObjectDescriptor.EndOffset, out WowContainerDescriptor obj))
            {
                RawWowContainer = obj;
            }
        }
    }
}