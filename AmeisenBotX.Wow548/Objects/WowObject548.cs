using AmeisenBotX.Common.Math;
using AmeisenBotX.Wow;
using AmeisenBotX.Wow.Objects;
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

        protected WowMemoryApi Memory { get; private set; }

        public virtual void Init(WowMemoryApi memory, IntPtr baseAddress, IntPtr descriptorAddress)
        {
            Memory = memory;
            BaseAddress = baseAddress;
            DescriptorAddress = descriptorAddress;
            Update(memory);
        }

        public virtual void Update(WowMemoryApi memory)
        {
        }

        protected WowObjectDescriptor548 GetObjectDescriptor()
        {
            return ObjectDescriptor ??= Memory.Read(DescriptorAddress, out WowObjectDescriptor548 objPtr) ? objPtr : new();
        }
    }
}