using System.Runtime.InteropServices;

namespace AmeisenBotX.Core.Data.Objects.WowObject.Structs
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