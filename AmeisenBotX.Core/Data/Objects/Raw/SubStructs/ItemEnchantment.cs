using System.Runtime.InteropServices;

namespace AmeisenBotX.Core.Data.Objects.Raw.SubStructs
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