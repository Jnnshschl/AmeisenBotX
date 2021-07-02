using System.Runtime.InteropServices;

namespace AmeisenBotX.Wow.Objects.SubStructs
{
    [StructLayout(LayoutKind.Sequential)]
    public record ItemEnchantment
    {
        public int Id { get; set; }

        public int Duration { get; set; }

        public short C { get; set; }

        public short D { get; set; }
    }
}