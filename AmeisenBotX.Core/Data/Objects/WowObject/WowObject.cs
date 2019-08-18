using System;

namespace AmeisenBotX.Core.Data.Objects.WowObject
{
    public class WowObject
    {
        public IntPtr BaseAddress { get; set; }
        public IntPtr DescriptorAddress { get; set; }
        public ulong Guid { get; set; }
        public WowObjectType Type { get; set; }
    }
}