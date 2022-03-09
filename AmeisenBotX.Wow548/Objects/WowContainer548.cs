using AmeisenBotX.Memory;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Offsets;
using AmeisenBotX.Wow548.Objects.Descriptors;

namespace AmeisenBotX.Wow548.Objects
{
    [Serializable]
    public unsafe class WowContainer548 : WowObject548, IWowContainer
    {
        public int SlotCount => RawWowContainer.SlotCount;

        protected WowContainerDescriptor548 RawWowContainer { get; private set; }

        public override string ToString()
        {
            return $"Container: [{Guid}] SlotCount: {SlotCount}";
        }

        public override void Update(IMemoryApi memoryApi, IOffsetList offsetList)
        {
            base.Update(memoryApi, offsetList);

            if (memoryApi.Read(DescriptorAddress + sizeof(WowObjectDescriptor548), out WowContainerDescriptor548 obj))
            {
                RawWowContainer = obj;
            }
        }
    }
}