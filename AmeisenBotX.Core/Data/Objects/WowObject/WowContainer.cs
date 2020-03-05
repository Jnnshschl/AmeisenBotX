using AmeisenBotX.Core.Data.Objects.WowObject.Structs;
using AmeisenBotX.Memory;

namespace AmeisenBotX.Core.Data.Objects.WowObject
{
    public class WowContainer : WowObject
    {
        public override int BaseOffset => RawWowObject.EndOffset;

        private RawWowContainer RawWowContainer { get; set; }

        public override WowObject Update(XMemory xMemory)
        {
            if (xMemory.ReadStruct(BaseAddress, out RawWowContainer rawWowContainer))
            {
                RawWowContainer = rawWowContainer;
            }

            return this;
        }
    }
}