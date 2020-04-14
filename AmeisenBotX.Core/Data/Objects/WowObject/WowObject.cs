using AmeisenBotX.Core.Data.Objects.WowObject.Structs;
using AmeisenBotX.Memory;
using AmeisenBotX.Pathfinding.Objects;
using System;

namespace AmeisenBotX.Core.Data.Objects.WowObject
{
    public class WowObject
    {
        // W.I.P
        // This is going to reduce the ReadProcessMemory calls a lot
        public WowObject(IntPtr baseAddress, WowObjectType type)
        {
            BaseAddress = baseAddress;
            Type = type;
        }

        public IntPtr BaseAddress { get; private set; }

        public IntPtr DescriptorAddress { get; set; }

        public int EntryId => RawWowObject.EntryId;

        public ulong Guid => RawWowObject.Guid;

        public Vector3 Position { get; set; }

        public float Scale => RawWowObject.Scale;

        public WowObjectType Type { get; private set; }

        private RawWowObject RawWowObject { get; set; }

        public override string ToString()
            => $"Object: {Guid}";

        public WowObject UpdateRawWowObject(XMemory xMemory)
        {
            if (xMemory.ReadStruct(DescriptorAddress, out RawWowObject rawWowObject))
            {
                RawWowObject = rawWowObject;
            }

            return this;
        }
    }
}