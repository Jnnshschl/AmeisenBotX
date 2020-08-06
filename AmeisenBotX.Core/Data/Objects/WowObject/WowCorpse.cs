using AmeisenBotX.Core.Data.Objects.WowObject.Structs;
using AmeisenBotX.Memory;
using System;

namespace AmeisenBotX.Core.Data.Objects.WowObject
{
    [Serializable]
    public class WowCorpse : WowObject
    {
        public WowCorpse(IntPtr baseAddress, WowObjectType type) : base(baseAddress, type)
        {
        }

        public int DisplayId { get; set; }

        public ulong Owner { get; set; }

        public ulong Party { get; set; }

        public override string ToString()
        {
            return $"Corpse: [{Guid}] Owner: {Owner} Party: {Party} DisplayId: {DisplayId}";
        }

        public WowCorpse UpdateRawWowCorpse()
        {
            UpdateRawWowObject();

            unsafe
            {
                fixed (RawWowCorpse* objPtr = stackalloc RawWowCorpse[1])
                {
                    if (WowInterface.I.XMemory.ReadStruct(DescriptorAddress + RawWowObject.EndOffset, objPtr))
                    {
                        DisplayId = objPtr[0].DisplayId;
                        Owner = objPtr[0].Owner;
                        Party = objPtr[0].Party;
                    }
                }
            }

            return this;
        }
    }
}