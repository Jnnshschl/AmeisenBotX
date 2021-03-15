using System.Runtime.InteropServices;

namespace AmeisenBotX.Core.Data.Objects.Raw
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RawWowAura
    {
        public ulong Creator;
        public int SpellId;
        public byte Flags;
        public byte Level;
        public byte StackCount;
        public byte Unknown;
        public uint Duration;
        public uint EndTime;
    }
}