using System.Runtime.InteropServices;

namespace AmeisenBotX.Wow335a.Objects.Raw
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RawWowObject
    {
        public ulong Guid;
        public int Type;
        public int EntryId;
        public float Scale;
        public int WowObjectPad;

        public static readonly int EndOffset = 24;
    }
}