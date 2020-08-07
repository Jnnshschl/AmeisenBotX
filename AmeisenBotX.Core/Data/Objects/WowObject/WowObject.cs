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
            fixed (RawWowObject* objPtr = stackalloc RawWowObject[1])
            {
                if (WowInterface.I.XMemory.ReadStruct(DescriptorAddress, objPtr))
                {
                    EntryId = objPtr[0].EntryId;
                    Guid = objPtr[0].Guid;
                    Scale = objPtr[0].Scale;
                }
            }
        }
    }
}