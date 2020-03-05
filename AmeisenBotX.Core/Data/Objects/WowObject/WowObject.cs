using AmeisenBotX.Core.Data.Objects.WowObject.Structs;
using AmeisenBotX.Memory;
using AmeisenBotX.Pathfinding.Objects;
using System;

namespace AmeisenBotX.Core.Data.Objects.WowObject
{
    public class WowObject
    {
        public IntPtr BaseAddress { get; set; }

        public virtual int BaseOffset => 0x0;

        public IntPtr DescriptorAddress { get; set; }

        public int EntryId { get; set; }

        public ulong Guid { get; set; }

        public Vector3 Position { get; set; }

        public float Scale { get; set; }

        public WowObjectType Type { get; set; }

        // W.I.P
        // This is going to reduce the ReadProcessMemory calls a lot
        // public WowObject(IntPtr baseAddress, WowObjectType type = WowObjectType.None)
        // {
        //     BaseAddress = baseAddress;
        //     Type = type;
        // }
        //
        // public IntPtr BaseAddress { get; private set; }
        //
        // public int EntryId => RawWowObject.EntryId;
        //
        // public ulong Guid => RawWowObject.Guid;
        //
        // public float Scale => RawWowObject.Scale;
        //
        // public WowObjectType Type { get; private set; }
        private RawWowObject RawWowObject { get; set; }

        public virtual WowObject Update(XMemory xMemory)
        {
            if (xMemory.ReadStruct(BaseAddress, out RawWowObject rawWowObject))
            {
                RawWowObject = rawWowObject;
            }

            return this;
        }
    }
}