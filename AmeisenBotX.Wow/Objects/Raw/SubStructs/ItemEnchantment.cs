using System.Runtime.InteropServices;

namespace AmeisenBotX.Wow.Objects.SubStructs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ItemEnchantment
    {
        public int Id;
        public int Duration;
        public short c;
        public short d;
    }
}