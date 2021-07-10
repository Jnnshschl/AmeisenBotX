using System.Runtime.InteropServices;

namespace AmeisenBotX.Wow335a.Objects.Descriptors
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct WowDynobjectDescriptor
    {
        public ulong Caster;
        public fixed byte DynobjectBytes[4];
        public int SpellId;
        public float Radius;
        public int CastTime;

        public static readonly int EndOffset = 24;
    }
}