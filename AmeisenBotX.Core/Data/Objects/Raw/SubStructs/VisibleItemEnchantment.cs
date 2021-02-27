using System.Runtime.InteropServices;

namespace AmeisenBotX.Core.Data.Objects.Raw.SubStructs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VisibleItemEnchantment
    {
        public int Id;
        public short c;
        public short d;
    }
}