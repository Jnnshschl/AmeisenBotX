using AmeisenBotX.Core.Data.Enums;
using System;

namespace AmeisenBotX.Core.Data.Objects.Raw
{
    [Serializable]
    public class WowAura
    {
        public WowAura(RawWowAura rawWowAura, string name)
        {
            RawWowAura = rawWowAura;
            Name = name;
        }

        public ulong Creator => RawWowAura.Creator;

        public uint Duration => RawWowAura.Duration;

        public uint EndTime => RawWowAura.EndTime;

        public byte Flags => RawWowAura.Flags;

        public bool IsActive => ((WowAuraFlags)Flags).HasFlag(WowAuraFlags.Active);

        public bool IsHarmful => ((WowAuraFlags)Flags).HasFlag(WowAuraFlags.Harmful);

        public bool IsPassive => ((WowAuraFlags)Flags).HasFlag(WowAuraFlags.Passive);

        public byte Level => RawWowAura.Level;

        public string Name { get; }

        public int SpellId => RawWowAura.SpellId;

        public byte StackCount => RawWowAura.StackCount;

        private RawWowAura RawWowAura { get; }

        public override string ToString()
        {
            return $"{Name} ({SpellId}) (lvl. {Level}) x{StackCount} [CG: {Creator}], Harmful: {IsHarmful}, Passive: {IsPassive}";
        }
    }
}