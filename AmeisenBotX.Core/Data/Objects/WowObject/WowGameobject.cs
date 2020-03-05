using AmeisenBotX.Core.Data.Objects.WowObject.Structs;
using AmeisenBotX.Memory;
using System.Collections.Specialized;

namespace AmeisenBotX.Core.Data.Objects.WowObject
{
    public class WowGameobject : WowObject
    {
        public override int BaseOffset => base.BaseOffset + RawWowObject.EndOffset;

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

        public override WowObject Update(XMemory xMemory)
        {
            if (xMemory.ReadStruct(BaseAddress, out RawWowGameobject rawWowGameobject))
            {
                RawWowGameobject = rawWowGameobject;
            }

            return this;
        }
    }
}