using System.Runtime.InteropServices;

namespace AmeisenBotX.Wow548.Objects.Descriptors
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct WowCorpseDescriptor548
    {
        public ulong Owner;
        public ulong Party;
        public int DisplayId;
        public fixed int Items[19];
        public int SkinId;
        public int FacialHairStyle;
        public int Flags;
        public int DynamicFlags;
    }
}