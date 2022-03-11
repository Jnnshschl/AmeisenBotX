using AmeisenBotX.Wow.Combatlog.Enums;

namespace AmeisenBotX.Wow548.Combatlog.Enums
{
    public class CombatlogFields548 : ICombatlogFields
    {
        public int DestinationGuid { get; } = 7;

        public int DestinationName { get; } = 8;

        public int Flags { get; } = 5;

        public int Source { get; } = 3;

        public int SourceName { get; } = 4;

        public int SpellAmount { get; } = 14;

        public int SpellAmountOver { get; } = 15;

        public int SpellSpellId { get; } = 11;

        public int SwingDamageAmount { get; } = 14;

        public int TargetFlags { get; } = 9;

        public int Timestamp { get; } = 0;

        public int Type { get; } = 1;
    }
}