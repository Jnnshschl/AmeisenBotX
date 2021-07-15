using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using AmeisenBotX.Wow.Combatlog.Enums;
using AmeisenBotX.Wow.Combatlog.Objects;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace AmeisenBotX.Wow.Combatlog
{
    public class DefaultCombatLogParser : ICombatLogParser
    {
        public DefaultCombatLogParser()
        {
        }

        public event Action<ulong, ulong, int> OnDamage;

        public event Action<ulong, ulong> OnPartyKill;

        public event Action<ulong> OnUnitDied;

        public void Parse(long timestamp, List<string> args)
        {
            AmeisenLogger.I.Log("CombatLogParser", $"[{timestamp}] Parsing CombatLog: {JsonSerializer.Serialize(args)}", LogLevel.Verbose);

            if (BasicCombatLogEntry.TryParse(args, out BasicCombatLogEntry entry))
            {
                switch (entry.Type)
                {
                    case CombatLogEntryType.PARTY:
                        switch (entry.Subtype)
                        {
                            case CombatLogEntrySubtype.KILL:
                                AmeisenLogger.I.Log("CombatLogParser", $"OnPartyKill({entry.SourceGuid}, {entry.DestinationGuid})");
                                OnPartyKill?.Invoke(entry.SourceGuid, entry.DestinationGuid);
                                break;
                        }
                        break;

                    case CombatLogEntryType.UNIT:
                        switch (entry.Subtype)
                        {
                            case CombatLogEntrySubtype.DIED:
                                AmeisenLogger.I.Log("CombatLogParser", $"OnUnitDied({entry.SourceGuid})");
                                OnUnitDied?.Invoke(entry.SourceGuid);
                                break;
                        }
                        break;
                }

                switch (entry.Subtype)
                {
                    case CombatLogEntrySubtype.DAMAGE:
                        if (int.TryParse(entry.Args[(int)CombatLogField.Damage], out int damage))
                        {
                            AmeisenLogger.I.Log("CombatLogParser", $"OnDamage({entry.SourceGuid}, {entry.DestinationGuid}, {entry.Args[(int)CombatLogField.Damage]})");
                            OnDamage?.Invoke(entry.SourceGuid, entry.DestinationGuid, damage);
                        }
                        break;
                }
            }
            else
            {
                AmeisenLogger.I.Log("CombatLogParser", $"Parsing failed", LogLevel.Verbose);
            }
        }
    }
}