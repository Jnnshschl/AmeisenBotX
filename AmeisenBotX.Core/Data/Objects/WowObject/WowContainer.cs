using AmeisenBotX.Core.Data.Objects.WowObject.Structs;
using AmeisenBotX.Memory;
using System;

namespace AmeisenBotX.Core.Data.Objects.WowObject
{
    public class WowContainer : WowObject
    {
        public WowContainer(IntPtr baseAddress, WowObjectType type) : base(baseAddress, type)
        {

        }
        
        private RawWowContainer RawWowContainer { get; set; }

        public WowContainer UpdateRawWowContainer(XMemory xMemory)
        {
            UpdateRawWowObject(xMemory);

            if (xMemory.ReadStruct(DescriptorAddress + RawWowObject.EndOffset, out RawWowContainer rawWowContainer))
            {
                RawWowContainer = rawWowContainer;
            }

            return this;
        }
    }
}