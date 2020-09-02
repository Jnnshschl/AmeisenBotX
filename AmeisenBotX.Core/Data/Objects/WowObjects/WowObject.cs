using AmeisenBotX.Core.Data.Objects.WowObjects.Structs;
using System;
using System.Numerics;

namespace AmeisenBotX.Core.Data.Objects.WowObjects
{
    public class WowObject
    {
        public WowObject(IntPtr baseAddress, WowObjectType type, IntPtr descriptorAddress)
        {
            BaseAddress = baseAddress;
            Type = type;
            DescriptorAddress = descriptorAddress;
        }

        public IntPtr BaseAddress { get; private set; }

        public IntPtr DescriptorAddress { get; set; }

        public int EntryId { get; set; }

        public ulong Guid { get; set; }

        public Vector3 Position { get; set; }

        public float Scale { get; set; }

        public WowObjectType Type { get; private set; }

        public override string ToString()
        {
            return $"Object: {Guid}";
        }

        public virtual unsafe void Update()
        {
            if (WowInterface.I.XMemory.ReadStruct(DescriptorAddress, out RawWowObject objPtr))
            {
                EntryId = objPtr.EntryId;
                Guid = objPtr.Guid;
                Scale = objPtr.Scale;
            }
        }
    }
}