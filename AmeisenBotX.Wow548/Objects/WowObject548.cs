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
        public IntPtr BaseAddress { get; private set; }

        public IntPtr DescriptorAddress { get; private set; }

        public int EntryId => RawObject.EntryId;

        public ulong Guid => RawObject.Guid;

        public Vector3 Position { get; protected set; }

        public float Scale => RawObject.Scale;

        public BitVector32 UnitFlagsDynamic => RawObject.DynamicFlags;

        protected WowObjectDescriptor548 RawObject { get; private set; }

        public virtual void Init(IMemoryApi memoryApi, IOffsetList offsetList, IntPtr baseAddress, IntPtr descriptorAddress)
        {
            BaseAddress = baseAddress;
            DescriptorAddress = descriptorAddress;
            Update(memoryApi, offsetList);
        }

        public virtual void Update(IMemoryApi memoryApi, IOffsetList offsetList)
        {
            if (DescriptorAddress != IntPtr.Zero && memoryApi.Read(DescriptorAddress, out WowObjectDescriptor548 obj))
            {
                RawObject = obj;
            }
        }
    }
}