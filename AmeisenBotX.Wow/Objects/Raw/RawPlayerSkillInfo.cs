using System.Runtime.InteropServices;

namespace AmeisenBotX.Wow.Objects
{
    [StructLayout(LayoutKind.Sequential)]
    public record RawPlayerSkillInfo
    {
        public ushort Id { get; set; }

        public ushort Bonus { get; set; }

        public ushort MaxValue { get; set; }

        public short Modifier { get; set; }

        public ushort SkillStep { get; set; }

        public ushort Value { get; set; }
    }
}