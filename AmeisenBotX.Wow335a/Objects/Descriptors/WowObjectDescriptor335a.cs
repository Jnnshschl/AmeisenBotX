using System.Runtime.InteropServices;

namespace AmeisenBotX.Wow335a.Objects.Descriptors
{
    [StructLayout(LayoutKind.Sequential)]
    public struct WowObjectDescriptor335a
    {
        public ulong Guid;
        public int Type;
        public int EntryId;
        public float Scale;
        public int WowObjectPad;

        public static readonly int EndOffset = 24;
    }
}