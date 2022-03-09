using System.Collections.Specialized;
using System.Runtime.InteropServices;

namespace AmeisenBotX.Wow548.Objects.Descriptors
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct WowObjectDescriptor548
    {
        public ulong Guid;
        public ulong Data;
        public int Type;
        public int EntryId;
        public BitVector32 DynamicFlags;
        public float Scale;
    }
}