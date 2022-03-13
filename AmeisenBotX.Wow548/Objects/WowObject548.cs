using AmeisenBotX.Common.Math;
using AmeisenBotX.Memory;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Offsets;
using AmeisenBotX.Wow548.Objects.Descriptors;
using System.Collections.Specialized;

namespace AmeisenBotX.Wow548.Objects
{
    [Serializable]
    public class WowObject548 : IWowObject
    {
        protected WowObjectDescriptor548? ObjectDescriptor;

        public IntPtr BaseAddress { get; private set; }

        public IntPtr DescriptorAddress { get; private set; }

        public int EntryId => GetObjectDescriptor().EntryId;

        public ulong Guid => GetObjectDescriptor().Guid;

        public Vector3 Position { get; protected set; }

        public float Scale => GetObjectDescriptor().Scale;

        public BitVector32 UnitFlagsDynamic => GetObjectDescriptor().DynamicFlags;

        protected IMemoryApi Memory { get; private set; }

        protected IOffsetList Offsets { get; private set; }

        public virtual void Init(IMemoryApi memoryApi, IOffsetList offsetList, IntPtr baseAddress, IntPtr descriptorAddress)
        {
            Memory = memoryApi;
            Offsets = offsetList;
            BaseAddress = baseAddress;
            DescriptorAddress = descriptorAddress;
            Update(memoryApi, offsetList);
        }

        public virtual void Update(IMemoryApi memoryApi, IOffsetList offsetList)
        {
        }

        protected WowObjectDescriptor548 GetObjectDescriptor() => ObjectDescriptor ??= Memory.Read(DescriptorAddress, out WowObjectDescriptor548 objPtr) ? objPtr : new();
    }
}