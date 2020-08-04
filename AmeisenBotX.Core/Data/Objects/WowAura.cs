using AmeisenBotX.Core.Data.Objects.Enums;
using AmeisenBotX.Core.Data.Objects.Structs;

namespace AmeisenBotX.Core.Data.Objects
{
    public class WowAura
    {
        private string name = string.Empty;

        public WowAura(WowInterface wowInterface, RawWowAura rawWowAura)
        {
            WowInterface = wowInterface;
            RawWowAura = rawWowAura;
        }

        public ulong CreatorGuid => RawWowAura.Creator;

        public uint Duration => RawWowAura.Duration;

        public uint EndTime => RawWowAura.EndTime;

        public AuraFlags Flags => (AuraFlags)RawWowAura.Flags;

        public bool IsActive => Flags.HasFlag(AuraFlags.Active);

        public bool IsHarmful => Flags.HasFlag(AuraFlags.Harmful);

        public bool IsPassive => Flags.HasFlag(AuraFlags.Passive);

        public byte Level => RawWowAura.Level;

        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(name) && !WowInterface.BotCache.TryGetSpellName(SpellId, out name))
                {
                    name = WowInterface.HookManager.GetSpellNameById(SpellId);
                    WowInterface.BotCache.CacheSpellName(SpellId, name);
                }

                return name;
            }
            set { name = value; }
        }

        public int SpellId => RawWowAura.SpellId;

        public int StackCount => RawWowAura.StackCount;

        private RawWowAura RawWowAura { get; set; }

        private WowInterface WowInterface { get; }

        public override string ToString()
            => $"{SpellId} (lvl. {Level}) x{StackCount} [CG: {CreatorGuid}], Harmful: {IsHarmful}, Passive: {IsPassive}";
    }
}