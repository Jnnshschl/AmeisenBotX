using AmeisenBotX.Memory;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Offsets;
using AmeisenBotX.Wow548.Objects.Descriptors;

namespace AmeisenBotX.Wow548.Objects
{
    [Serializable]
    public unsafe class WowContainer548 : WowObject548, IWowContainer
    {
        protected WowContainerDescriptor548? ContainerDescriptor;

        public int SlotCount => GetContainerDescriptor().NumSlots;

        public override string ToString()
        {
            return $"Container: [{Guid}] SlotCount: {SlotCount}";
        }

        public override void Update(IMemoryApi memoryApi, IOffsetList offsetList)
        {
            base.Update(memoryApi, offsetList);
        }

        protected WowContainerDescriptor548 GetContainerDescriptor() => ContainerDescriptor ??= Memory.Read(DescriptorAddress + sizeof(WowObjectDescriptor548), out WowContainerDescriptor548 objPtr) ? objPtr : new();
    }
}