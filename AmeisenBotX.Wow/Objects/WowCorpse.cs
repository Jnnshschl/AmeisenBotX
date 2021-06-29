using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Wow.Objects;
using System;
using AmeisenBotX.Memory;
using AmeisenBotX.Common.Offsets;

namespace AmeisenBotX.Core.Data.Objects
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

        public override void Update(XMemory xMemory, IOffsetList offsetList)
        {
            base.Update(xMemory, offsetList);

            if (xMemory.Read(DescriptorAddress + RawWowObject.EndOffset, out RawWowCorpse objPtr))
            {
                DisplayId = objPtr.DisplayId;
                Owner = objPtr.Owner;
                Party = objPtr.Party;
            }
        }
    }
}