using AmeisenBotX.Core.Data.Objects.WowObject.Structs;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System;

namespace AmeisenBotX.Core.Data.Objects.WowObject
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

        public unsafe virtual void Update()
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