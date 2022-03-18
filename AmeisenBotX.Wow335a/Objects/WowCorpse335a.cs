using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow335a.Objects.Descriptors;
using System;

namespace AmeisenBotX.Wow335a.Objects
{
    [Serializable]
    public class WowCorpse335a : WowObject335a, IWowCorpse
    {
        public int DisplayId => RawWowCorpse.DisplayId;

        public ulong Owner => RawWowCorpse.Owner;

        public ulong Party => RawWowCorpse.Party;

        protected WowCorpseDescriptor335a RawWowCorpse { get; private set; }

        public override string ToString()
        {
            return $"Corpse: [{Guid}] Owner: {Owner} Party: {Party} DisplayId: {DisplayId}";
        }

        public override void Update()
        {
            base.Update();

            if (Memory.Read(DescriptorAddress + WowObjectDescriptor335a.EndOffset, out WowCorpseDescriptor335a obj))
            {
                RawWowCorpse = obj;
            }
        }
    }
}