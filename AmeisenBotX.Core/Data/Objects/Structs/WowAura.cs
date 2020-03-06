using System.Runtime.InteropServices;

namespace AmeisenBotX.Core.Data.Objects.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct WowAura
    {
        public ulong Creator;
        public int AuraId;
        public byte Flags;
        public byte Level;
        public ushort StackCount;
        public uint Duration;
        public uint EndTime;
    }
}