using System.Runtime.InteropServices;

namespace AmeisenBotX.Wow548.Objects.Descriptors
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct WowContainerDescriptor548
    {
        public fixed long Slots[36];
        public int SlotCount;

        public static readonly int EndOffset = sizeof(WowObjectDescriptor548) + sizeof(WowContainerDescriptor548);
    }
}