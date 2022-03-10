using AmeisenBotX.Memory;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Offsets;
using AmeisenBotX.Wow548.Objects.Descriptors;

namespace AmeisenBotX.Wow548.Objects
{
    [Serializable]
    public unsafe class WowCorpse548 : WowObject548, IWowCorpse
    {
        public int DisplayId => RawWowCorpse.DisplayId;

        public ulong Owner => RawWowCorpse.Owner;

        public ulong Party => RawWowCorpse.PartyGuid;

        protected WowCorpseDescriptor548 RawWowCorpse { get; private set; }

        public override string ToString()
        {
            return $"Corpse: [{Guid}] Owner: {Owner} Party: {Party} DisplayId: {DisplayId}";
        }

        public override void Update(IMemoryApi memoryApi, IOffsetList offsetList)
        {
            base.Update(memoryApi, offsetList);

            if (memoryApi.Read(DescriptorAddress + sizeof(WowObjectDescriptor548), out WowCorpseDescriptor548 obj))
            {
                RawWowCorpse = obj;
            }
        }
    }
}