using AmeisenBotX.Core.Data.Objects.WowObject.Structs;
using AmeisenBotX.Memory;

namespace AmeisenBotX.Core.Data.Objects.WowObject
{
    public class WowCorpse : WowObject
    {
        public override int BaseOffset => RawWowObject.EndOffset;

        private RawWowCorpse RawWowCorpse { get; set; }

        public override WowObject Update(XMemory xMemory)
        {
            if (xMemory.ReadStruct(BaseAddress, out RawWowCorpse rawWowCorpse))
            {
                RawWowCorpse = rawWowCorpse;
            }

            return this;
        }
    }
}