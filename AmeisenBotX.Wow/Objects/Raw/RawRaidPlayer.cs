using System.Runtime.InteropServices;

namespace AmeisenBotX.Wow.Objects
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct RawRaidPlayer
    {
        public ulong Guid;
        public fixed ulong Padding[9];
    }
}