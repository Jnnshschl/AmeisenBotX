using AmeisenBotX.Wow.Objects.Enums;
using System.Runtime.InteropServices;

namespace AmeisenBotX.Wow.Objects
{
    [StructLayout(LayoutKind.Sequential)]
    public record RawWowAura
    {
        public ulong Creator { get; set; }

        public int SpellId { get; set; }

        public byte Flags { get; set; }

        public byte Level { get; set; }

        public byte StackCount { get; set; }

        public byte Unknown { get; set; }

        public uint Duration { get; set; }

        public uint EndTime { get; set; }

        public bool IsActive => ((WowAuraFlags)Flags).HasFlag(WowAuraFlags.Active);

        public bool IsHarmful => ((WowAuraFlags)Flags).HasFlag(WowAuraFlags.Harmful);

        public bool IsPassive => ((WowAuraFlags)Flags).HasFlag(WowAuraFlags.Passive);

        public override string ToString()
        {
            return $"{SpellId} (lvl. {Level}) x{StackCount} [CG: {Creator}], Harmful: {IsHarmful}, Passive: {IsPassive}";
        }
    }
}