using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Memory;
using AmeisenBotX.Wow;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
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

        protected IMemoryApi Memory { get; private set; }

        protected WowObjectDescriptor335a RawObject { get; private set; }

        public virtual void Init(IMemoryApi memory, IntPtr baseAddress, IntPtr descriptorAddress)
        {
            Memory = memory;
            BaseAddress = baseAddress;
            DescriptorAddress = descriptorAddress;

            Update();
        }

        public override string ToString()
        {
            return $"Object: {Guid}";
        }

        public virtual void Update()
        {
            if (DescriptorAddress != IntPtr.Zero && Memory.Read(DescriptorAddress, out WowObjectDescriptor335a obj))
            {
                RawObject = obj;
            }
        }
    }
}