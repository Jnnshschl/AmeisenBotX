using AmeisenBotX.Common.Math;
using AmeisenBotX.Memory;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Wow.Offsets;
using AmeisenBotX.Wow548.Objects.Descriptors;
using System.Collections.Specialized;

namespace AmeisenBotX.Wow548.Objects
{
    [Serializable]
    public class WowObject548 : IWowObject
    {
        public WowObject548(IntPtr baseAddress, IntPtr descriptorAddress)
        {
            BaseAddress = baseAddress;
            DescriptorAddress = descriptorAddress;
            Type = WowObjectType.None;
        }

        public IntPtr BaseAddress { get; }

        public IntPtr DescriptorAddress { get; }

        public int EntryId => RawObject.EntryId;

        public ulong Guid => RawObject.Guid;

        public Vector3 Position { get; protected set; }

        public float Scale => RawObject.Scale;

        public WowObjectType Type { get; protected set; }

        public BitVector32 UnitFlagsDynamic => RawObject.DynamicFlags;

        protected WowObjectDescriptor548 RawObject { get; private set; }

        public override string ToString()
        {
            return $"Object: {Guid}";
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