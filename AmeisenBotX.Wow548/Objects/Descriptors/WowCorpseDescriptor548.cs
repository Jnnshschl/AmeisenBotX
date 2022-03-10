using System.Collections.Specialized;
using System.Runtime.InteropServices;

namespace AmeisenBotX.Wow548.Objects.Descriptors
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct WowCorpseDescriptor548
    {
        public ulong Owner;
        public ulong PartyGuid;
        public int DisplayId;
        public fixed int Items[19];
        public int SkinId;
        public int FacialHairStyleId;
        public BitVector32 Flags;
        public BitVector32 DynamicFlags;
    }
}