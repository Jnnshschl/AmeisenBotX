using AmeisenBotX.Common.Offsets;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Memory;
using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Wow335a.Objects.Descriptors;
using System;

namespace AmeisenBotX.Wow335a.Objects
{
    [Serializable]
    public class WowCorpse335a : WowObject335a, IWowCorpse
    {
        public WowCorpse335a(IntPtr baseAddress, IntPtr descriptorAddress) : base(baseAddress, descriptorAddress)
        {
            Type = WowObjectType.Corpse;
        }

        public int DisplayId => RawWowCorpse.DisplayId;

        public ulong Owner => RawWowCorpse.Owner;

        public ulong Party => RawWowCorpse.Party;

        protected WowCorpseDescriptor RawWowCorpse { get; private set; }

        public override string ToString()
        {
            return $"Corpse: [{Guid}] Owner: {Owner} Party: {Party} DisplayId: {DisplayId}";
        }

        public override void Update(IMemoryApi memoryApi, IOffsetList offsetList)
        {
            base.Update(memoryApi, offsetList);

            if (memoryApi.Read(DescriptorAddress + WowObjectDescriptor.EndOffset, out WowCorpseDescriptor obj))
            {
                RawWowCorpse = obj;
            }
        }
    }
}