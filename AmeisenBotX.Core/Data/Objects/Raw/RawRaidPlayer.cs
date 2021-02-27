using System.Runtime.InteropServices;

namespace AmeisenBotX.Core.Data.Objects.Raw
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct RawRaidPlayer
    {
        public ulong Guid;
        public fixed ulong Padding[9];
    }
}