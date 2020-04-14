using AmeisenBotX.Core.Data.Objects.WowObject.Structs;
using AmeisenBotX.Memory;
using System;

namespace AmeisenBotX.Core.Data.Objects.WowObject
{
    public class WowCorpse : WowObject
    {
        public WowCorpse(IntPtr baseAddress, WowObjectType type) : base(baseAddress, type)
        {
        }

        public int DisplayId => RawWowCorpse.DisplayId;

        public ulong Owner => RawWowCorpse.Owner;

        public ulong Party => RawWowCorpse.Party;

        private RawWowCorpse RawWowCorpse { get; set; }

        public override string ToString()
            => $"Corpse: [{Guid}] Owner: {Owner} Party: {Party} DisplayId: {DisplayId}";

        public WowCorpse UpdateRawWowCorpse(XMemory xMemory)
        {
            UpdateRawWowObject(xMemory);

            if (xMemory.ReadStruct(DescriptorAddress + RawWowObject.EndOffset, out RawWowCorpse rawWowCorpse))
            {
                RawWowCorpse = rawWowCorpse;
            }

            return this;
        }
    }
}