using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow548.Objects.Descriptors;

namespace AmeisenBotX.Wow548.Objects
{
    [Serializable]
    public unsafe class WowCorpse548 : WowObject548, IWowCorpse
    {
        protected WowCorpseDescriptor548? CorpseDescriptor;

        public int DisplayId => GetCorpseDescriptor().DisplayId;

        public ulong Owner => GetCorpseDescriptor().Owner;

        public ulong Party => GetCorpseDescriptor().PartyGuid;

        public override string ToString()
        {
            return $"Corpse: [{Guid}] Owner: {Owner} Party: {Party} DisplayId: {DisplayId}";
        }

        public override void Update()
        {
            base.Update();
        }

        protected WowCorpseDescriptor548 GetCorpseDescriptor()
        {
            return CorpseDescriptor ??= Memory.Read(DescriptorAddress + sizeof(WowObjectDescriptor548), out WowCorpseDescriptor548 objPtr) ? objPtr : new();
        }
    }
}