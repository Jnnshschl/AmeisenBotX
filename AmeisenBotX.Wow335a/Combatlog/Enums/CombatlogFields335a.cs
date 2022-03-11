using AmeisenBotX.Wow.Combatlog.Enums;

namespace AmeisenBotX.Wow335a.Combatlog.Enums
{
    public class CombatlogFields335a : ICombatlogFields
    {
        public int DestinationGuid { get; } = 5;

        public int DestinationName { get; } = 6;

        public int Flags { get; } = 4;

        public int Source { get; } = 2;

        public int SourceName { get; } = 3;

        public int SpellAmount { get; } = 11;

        public int SpellAmountOver { get; } = 12;

        public int SpellSpellId { get; } = 8;

        public int SwingDamageAmount { get; } = 8;

        public int TargetFlags { get; } = 7;

        public int Timestamp { get; } = 0;

        public int Type { get; } = 1;
    }
}