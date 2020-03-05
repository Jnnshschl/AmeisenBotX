using AmeisenBotX.Core.Data.Objects.WowObject.Structs;
using AmeisenBotX.Memory;

namespace AmeisenBotX.Core.Data.Objects.WowObject
{
    public class WowDynobject : WowObject
    {
        public override int BaseOffset => RawWowObject.EndOffset;

        public ulong CasterGuid { get; set; }

        public float Facing { get; set; }

        public float Radius { get; set; }

        public int SpellId { get; set; }

        private RawWowDynobject RawWowDynobject { get; set; }

        public override WowObject Update(XMemory xMemory)
        {
            if (xMemory.ReadStruct(BaseAddress, out RawWowDynobject rawWowDynobject))
            {
                RawWowDynobject = rawWowDynobject;
            }

            return this;
        }
    }
}