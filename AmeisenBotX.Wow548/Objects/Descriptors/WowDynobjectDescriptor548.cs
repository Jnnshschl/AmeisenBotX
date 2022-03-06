using System.Runtime.InteropServices;

namespace AmeisenBotX.Wow548.Objects.Descriptors
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct WowDynobjectDescriptor548
    {
        public ulong Caster;
        public int TypeAndVisualId;
        public int SpellId;
        public float Radius;
        public int CastTime;

        public static readonly int EndOffset = sizeof(WowObjectDescriptor548) + sizeof(WowDynobjectDescriptor548);
    }
}