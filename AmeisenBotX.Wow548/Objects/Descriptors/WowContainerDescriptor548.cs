using System.Runtime.InteropServices;

namespace AmeisenBotX.Wow548.Objects.Descriptors
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct WowContainerDescriptor548
    {
        public fixed int Slots[72];
        public int NumSlots;
    }
}