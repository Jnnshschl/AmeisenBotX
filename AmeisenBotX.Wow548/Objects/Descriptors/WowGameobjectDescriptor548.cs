using System.Runtime.InteropServices;

namespace AmeisenBotX.Wow548.Objects.Descriptors
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct WowGameobjectDescriptor548
    {
        public ulong CreatedBy;
        public int DisplayId;
        public int Flags;
        public fixed float ParentRotations[4];
        public int Faction;
        public int Level;
        public int Health;
        public int SpellId;

        public static readonly int EndOffset = sizeof(WowObjectDescriptor548) + sizeof(WowGameobjectDescriptor548);
    }
}