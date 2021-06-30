using AmeisenBotX.Wow.Objects.Enums;
using System.Runtime.InteropServices;

namespace AmeisenBotX.Wow.Objects
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RawWowAura
    {
        public ulong Creator;
        public int SpellId;
        public byte Flags;
        public byte Level;
        public byte StackCount;
        public byte Unknown;
        public uint Duration;
        public uint EndTime;

        public bool IsActive => ((WowAuraFlags)Flags).HasFlag(WowAuraFlags.Active);

        public bool IsHarmful => ((WowAuraFlags)Flags).HasFlag(WowAuraFlags.Harmful);

        public bool IsPassive => ((WowAuraFlags)Flags).HasFlag(WowAuraFlags.Passive);

        public override string ToString()
        {
            return $"{SpellId} (lvl. {Level}) x{StackCount} [CG: {Creator}], Harmful: {IsHarmful}, Passive: {IsPassive}";
        }
    }
}