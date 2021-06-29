using System.Runtime.InteropServices;

namespace AmeisenBotX.Wow.Objects.SubStructs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VisibleItemEnchantment
    {
        public int Id;
        public short c;
        public short d;
    }
}