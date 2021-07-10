using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Offsets;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Memory;
using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Wow335a.Objects.Raw;
using System;

namespace AmeisenBotX.Wow335a.Objects
{
    [Serializable]
    public class WowObject335a : IWowObject
    {
        public WowObject335a(IntPtr baseAddress, IntPtr descriptorAddress)
        {
            BaseAddress = baseAddress;
            DescriptorAddress = descriptorAddress;
        }

        public IntPtr BaseAddress { get; }

        public IntPtr DescriptorAddress { get; }

        public int EntryId => RawObject.EntryId;

        public ulong Guid => RawObject.Guid;

        public Vector3 Position { get; protected set; }

        public float Scale => RawObject.Scale;

        public WowObjectType Type => (WowObjectType)RawObject.Type;

        protected RawWowObject RawObject { get; private set; }

        public override string ToString()
        {
            return $"Object: {Guid}";
        }

        public virtual void Update(IMemoryApi memoryApi, IOffsetList offsetList)
        {
            if (memoryApi.Read(DescriptorAddress, out RawWowObject obj))
            {
                RawObject = obj;
            }
        }
    }
}