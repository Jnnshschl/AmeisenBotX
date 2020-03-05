using AmeisenBotX.Core.Data.Objects.WowObject.Structs;
using AmeisenBotX.Memory;
using System;
using System.Collections.Specialized;

namespace AmeisenBotX.Core.Data.Objects.WowObject
{
    public class WowGameobject : WowObject
    {
        public WowGameobject(IntPtr baseAddress, WowObjectType type) : base(baseAddress, type)
        {

        }

        public int DisplayId { get; set; }

        public BitVector32 DynamicFlags { get; set; }

        public float Facing { get; set; }

        public int Faction { get; set; }

        public BitVector32 Flags { get; set; }

        public WowGameobjectType GameobjectType { get; set; }

        public int Level { get; set; }

        public float Rotation { get; set; }

        public int State { get; set; }

        private RawWowGameobject RawWowGameobject { get; set; }

        public WowGameobject UpdateRawWowGameobject(XMemory xMemory)
        {
            UpdateRawWowObject(xMemory);

            if (xMemory.ReadStruct(DescriptorAddress + RawWowObject.EndOffset, out RawWowGameobject rawWowGameobject))
            {
                RawWowGameobject = rawWowGameobject;
            }

            return this;
        }
    }
}