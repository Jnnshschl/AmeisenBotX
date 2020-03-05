using System.Runtime.InteropServices;

namespace AmeisenBotX.Core.Data.Objects.WowObject.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct RawWowContainer
    {
        public int SlotCount;
        public fixed byte WowContainerPad[4];
        public fixed long Slots[36];

        public const int EndOffset = 296;
    }
}