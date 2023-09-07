namespace AmeisenBotX.Wow.Combatlog.Enums
{
    public interface ICombatlogFields
    {
        int DestinationGuid { get; }

        int DestinationName { get; }

        int Flags { get; }

        int Source { get; }

        int SourceName { get; }

        int SpellAmount { get; }

        int SpellAmountOver { get; }

        int SpellSpellId { get; }

        int SwingDamageAmount { get; }

        int TargetFlags { get; }

        int Timestamp { get; }

        int Type { get; }
    }
}