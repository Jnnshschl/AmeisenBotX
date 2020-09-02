using AmeisenBotX.Core.Data.Objects.WowObjects.Structs;
using System;

namespace AmeisenBotX.Core.Data.Objects.WowObjects
{
    [Serializable]
    public class WowCorpse : WowObject
    {
        public WowCorpse(IntPtr baseAddress, WowObjectType type, IntPtr descriptorAddress) : base(baseAddress, type, descriptorAddress)
        {
        }

        public int DisplayId { get; set; }

        public ulong Owner { get; set; }

        public ulong Party { get; set; }

        public override string ToString()
        {
            return $"Corpse: [{Guid}] Owner: {Owner} Party: {Party} DisplayId: {DisplayId}";
        }

        public override unsafe void Update()
        {
            base.Update();

            if (WowInterface.I.XMemory.ReadStruct(DescriptorAddress + RawWowObject.EndOffset, out RawWowCorpse objPtr))
            {
                DisplayId = objPtr.DisplayId;
                Owner = objPtr.Owner;
                Party = objPtr.Party;
            }
        }
    }
}