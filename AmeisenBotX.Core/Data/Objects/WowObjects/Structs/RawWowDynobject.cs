using System.Runtime.InteropServices;

namespace AmeisenBotX.Core.Data.Objects.WowObjects.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct RawWowDynobject
    {
        public ulong Caster;
        public fixed byte DynobjectBytes[4];
        public int SpellId;
        public float Radius;
        public int CastTime;

        public static readonly int EndOffset = 24;
    }
}