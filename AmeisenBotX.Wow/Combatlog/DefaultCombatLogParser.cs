using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using AmeisenBotX.Wow.Combatlog.Enums;
using AmeisenBotX.Wow.Combatlog.Objects;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace AmeisenBotX.Wow.Combatlog
{
    public class DefaultCombatlogParser : ICombatlogParser
    {
        public event Action<ulong, ulong, int, int, int> OnDamage;

        public event Action<ulong, ulong, int, int, int> OnHeal;

        public event Action<ulong, ulong> OnPartyKill;

        public event Action<ulong> OnUnitDied;

        public void Parse(long timestamp, List<string> args)
        {
            if (BasicCombatlogEntry.TryParse(args, out BasicCombatlogEntry entry))
            {
                AmeisenLogger.I.Log("CombatLogParser", $"[{timestamp}] Parsing CombatLog: {JsonSerializer.Serialize(args)}", LogLevel.Verbose);

                switch (entry.Type)
                {
                    case CombatlogEntryType.PARTY:
                        switch (entry.Subtype)
                        {
                            case CombatlogEntrySubtype.KILL:
                                AmeisenLogger.I.Log("CombatLogParser", $"OnPartyKill({entry.SourceGuid}, {entry.DestinationGuid})");
                                OnPartyKill?.Invoke(entry.SourceGuid, entry.DestinationGuid);
                                break;
                        }
                        break;

                    case CombatlogEntryType.UNIT:
                        switch (entry.Subtype)
                        {
                            case CombatlogEntrySubtype.DIED:
                                AmeisenLogger.I.Log("CombatLogParser", $"OnUnitDied({entry.SourceGuid})");
                                OnUnitDied?.Invoke(entry.SourceGuid);
                                break;
                        }
                        break;

                    case CombatlogEntryType.SWING:
                        switch (entry.Subtype)
                        {
                            case CombatlogEntrySubtype.DAMAGE:
                                if (int.TryParse(entry.Args[(int)CombatlogField.SwingDamageAmount], out int damage))
                                {
                                    AmeisenLogger.I.Log("CombatLogParser", $"OnDamage({entry.SourceGuid}, {entry.DestinationGuid}, {entry.Args[(int)CombatlogField.SwingDamageAmount]})");
                                    OnDamage?.Invoke(entry.SourceGuid, entry.DestinationGuid, -1, damage, 0);
                                }
                                break;
                        }
                        break;

                    case CombatlogEntryType.SPELL:
                        switch (entry.Subtype)
                        {
                            case CombatlogEntrySubtype.DAMAGE:
                                if (int.TryParse(entry.Args[(int)CombatlogField.SpellAmount], out int spellAmount)
                                    && int.TryParse(entry.Args[(int)CombatlogField.SpellAmountOver], out int spellAmountOver)
                                    && int.TryParse(entry.Args[(int)CombatlogField.SpellSpellId], out int spellSpellId))
                                {
                                    AmeisenLogger.I.Log("CombatLogParser", $"OnDamage({entry.SourceGuid}, {entry.DestinationGuid}, {entry.Args[(int)CombatlogField.SpellSpellId]}, {entry.Args[(int)CombatlogField.SpellAmount]}, {entry.Args[(int)CombatlogField.SpellAmountOver]})");
                                    OnDamage?.Invoke(entry.SourceGuid, entry.DestinationGuid, spellSpellId, spellAmount, spellAmountOver);
                                }
                                break;

                            case CombatlogEntrySubtype.HEAL:
                                if (int.TryParse(entry.Args[(int)CombatlogField.SpellAmount], out int spellAmount2)
                                    && int.TryParse(entry.Args[(int)CombatlogField.SpellAmountOver], out int spellAmountOver2)
                                    && int.TryParse(entry.Args[(int)CombatlogField.SpellSpellId], out int spellSpellId2))
                                {
                                    AmeisenLogger.I.Log("CombatLogParser", $"OnHeal({entry.SourceGuid}, {entry.DestinationGuid}, {entry.Args[(int)CombatlogField.SpellSpellId]}, {entry.Args[(int)CombatlogField.SpellAmount]}, {entry.Args[(int)CombatlogField.SpellAmountOver]})");
                                    OnHeal?.Invoke(entry.SourceGuid, entry.DestinationGuid, spellSpellId2, spellAmount2, spellAmountOver2);
                                }
                                break;
                        }
                        break;
                }
            }
            else
            {
                AmeisenLogger.I.Log("CombatLogParser", $"Parsing failed: {JsonSerializer.Serialize(args)}", LogLevel.Warning);
            }
        }
    }
}