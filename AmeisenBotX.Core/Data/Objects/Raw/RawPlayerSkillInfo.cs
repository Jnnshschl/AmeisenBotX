using System.Runtime.InteropServices;

namespace AmeisenBotX.Core.Data.Objects.Raw
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RawPlayerSkillInfo
    {
        public ushort Id;
        public ushort Bonus;
        public ushort MaxValue;
        public short Modifier;
        public ushort SkillStep;
        public ushort Value;
    }
}