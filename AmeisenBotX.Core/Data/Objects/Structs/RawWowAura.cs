using System.Runtime.InteropServices;

namespace AmeisenBotX.Core.Data.Objects.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RawWowAura
    {
        public ulong Creator;
        public int SpellId;
        public byte Flags;
        public byte Level;
        public ushort StackCount;
        public uint Duration;
        public uint EndTime;
    }
}