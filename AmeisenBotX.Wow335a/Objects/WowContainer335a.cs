using AmeisenBotX.Common.Offsets;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Memory;
using AmeisenBotX.Wow335a.Objects.Raw;
using System;

namespace AmeisenBotX.Wow335a.Objects
{
    [Serializable]
    public class WowContainer335a : WowObject335a, IWowContainer
    {
        public WowContainer335a(IntPtr baseAddress, IntPtr descriptorAddress) : base(baseAddress, descriptorAddress)
        {
        }

        public int SlotCount => RawWowContainer.SlotCount;

        protected RawWowContainer RawWowContainer { get; private set; }

        public override string ToString()
        {
            return $"Container: [{Guid}] SlotCount: {SlotCount}";
        }

        public override void Update(IMemoryApi memoryApi, IOffsetList offsetList)
        {
            base.Update(memoryApi, offsetList);

            if (memoryApi.Read(DescriptorAddress + RawWowObject.EndOffset, out RawWowContainer obj))
            {
                RawWowContainer = obj;
            }
        }
    }
}