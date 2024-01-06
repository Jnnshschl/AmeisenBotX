using AmeisenBotX.Common.Math;
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
        public nint BaseAddress { get; private set; }

        public nint DescriptorAddress { get; private set; }

        public int EntryId => RawObject.EntryId;

        public ulong Guid => RawObject.Guid;

        public Vector3 Position { get; protected set; }

        public float Scale => RawObject.Scale;

        public WowObjectType Type { get; protected set; }

        protected WowMemoryApi Memory { get; private set; }

        protected WowObjectDescriptor335a RawObject { get; private set; }

        public virtual void Init(WowMemoryApi memory, nint baseAddress, nint descriptorAddress)
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
            if (DescriptorAddress != nint.Zero && Memory.Read(DescriptorAddress, out WowObjectDescriptor335a obj))
            {
                RawObject = obj;
            }
        }
    }
}