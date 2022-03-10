namespace AmeisenBotX.Wow.Combatlog.Enums
{
    public interface ICombatlogFields
    {
        int Timestamp { get; }

        int Type { get; }

        int Source { get; }

        int SourceName { get; }

        int Flags { get; }

        int DestinationGuid { get; }

        int DestinationName { get; }

        int TargetFlags { get; }

        int SwingDamageAmount { get; }

        int SpellSpellId { get; }

        int SpellAmount { get; }

        int SpellAmountOver { get; }
    }
}
