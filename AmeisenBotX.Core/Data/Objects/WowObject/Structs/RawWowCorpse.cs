using System.Runtime.InteropServices;

namespace AmeisenBotX.Core.Data.Objects.WowObject.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct RawWowCorpse
    {
        public ulong Owner;
        public ulong Party;
        public int DisplayId;
        public fixed int Items[19];
        public fixed byte CorpseBytes0[4];
        public fixed byte CorpseBytes1[4];
        public int Guild;
        public int Flags;
        public int DynamicFlags;
        public int WowCorpsePad;

        public static readonly int EndOffset = 120;
    }
}