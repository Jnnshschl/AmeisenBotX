using System.Runtime.InteropServices;

namespace AmeisenBotX.Wow.Objects.SubStructs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VisibleItemEnchantment
    {
        public int Id { get; set; }

        public short C { get; set; }

        public short D { get; set; }
    }
}