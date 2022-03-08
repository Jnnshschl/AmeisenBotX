using AmeisenBotX.Common.Math;
using AmeisenBotX.Memory;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Wow.Offsets;
using AmeisenBotX.Wow335a.Objects.Descriptors;
using System;

namespace AmeisenBotX.Wow335a.Objects
{
    [Serializable]
    public class WowObject335a : IWowObject
    {
        public IntPtr BaseAddress { get; private set; }

        public IntPtr DescriptorAddress { get; private set; }

        public int EntryId => RawObject.EntryId;

        public ulong Guid => RawObject.Guid;

        public Vector3 Position { get; protected set; }

        public float Scale => RawObject.Scale;

        public WowObjectType Type { get; protected set; }

        protected WowObjectDescriptor335a RawObject { get; private set; }

        public override string ToString()
        {
            return $"Object: {Guid}";
        }

        public virtual void Init(IMemoryApi memoryApi, IOffsetList offsetList, IntPtr baseAddress, IntPtr descriptorAddress)
        {
            BaseAddress = baseAddress;
            DescriptorAddress = descriptorAddress;
            Update(memoryApi, offsetList);
        }

        public virtual void Update(IMemoryApi memoryApi, IOffsetList offsetList)
        {
            if (DescriptorAddress != IntPtr.Zero && memoryApi.Read(DescriptorAddress, out WowObjectDescriptor335a obj))
            {
                RawObject = obj;
            }
        }
    }
}