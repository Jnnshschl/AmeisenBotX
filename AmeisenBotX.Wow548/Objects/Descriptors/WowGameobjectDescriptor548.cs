using System.Collections.Specialized;
using System.Runtime.InteropServices;

namespace AmeisenBotX.Wow548.Objects.Descriptors
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct WowGameobjectDescriptor548
    {
        public ulong CreatedBy;
        public int DisplayId;
        public BitVector32 Flags;
        public fixed float ParentRotation[4];
        public int FactionTemplate;
        public int Level;
        public int PercentHealth;
        public int StateSpellVisualId;
    }
}