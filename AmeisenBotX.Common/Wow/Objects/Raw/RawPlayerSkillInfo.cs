using System.Runtime.InteropServices;

namespace AmeisenBotX.Wow.Objects.Raw
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RawPlayerSkillInfo
    {
        public ushort Id { get; set; }

        public ushort Bonus { get; set; }

        public ushort MaxValue { get; set; }

        public short Modifier { get; set; }

        public ushort SkillStep { get; set; }

        public ushort Value { get; set; }
    }
}