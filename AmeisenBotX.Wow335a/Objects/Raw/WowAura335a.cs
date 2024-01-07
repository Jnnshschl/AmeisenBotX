using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Flags;
using System.Runtime.InteropServices;

namespace AmeisenBotX.Wow335a.Objects.Raw
{
    [StructLayout(LayoutKind.Sequential)]
    public struct WowAura335a : IWowAura
    {
        public ulong Creator { get; set; }

        public int SpellId { get; set; }

        public byte Flags { get; set; }

        public byte Level { get; set; }

        public byte StackCount { get; set; }

        public byte Unknown { get; set; }

        public uint Duration { get; set; }

        public uint EndTime { get; set; }

        public readonly bool IsActive => ((WowAuraFlag)Flags).HasFlag(WowAuraFlag.Active);

        public readonly bool IsHarmful => ((WowAuraFlag)Flags).HasFlag(WowAuraFlag.Harmful);

        public readonly bool IsPassive => ((WowAuraFlag)Flags).HasFlag(WowAuraFlag.Passive);

        public override readonly string ToString()
        {
            return $"{SpellId} (lvl. {Level}) x{StackCount} [CG: {Creator}], Harmful: {IsHarmful}, Passive: {IsPassive}";
        }
    }
}