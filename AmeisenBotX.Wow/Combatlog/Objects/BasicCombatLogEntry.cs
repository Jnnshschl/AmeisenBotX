using AmeisenBotX.Wow.Combatlog.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace AmeisenBotX.Wow.Combatlog.Objects
{
    [Serializable]
    public record BasicCombatlogEntry
    {
        public List<string> Args { get; set; }

        public ulong DestinationGuid { get; set; }

        public string DestinationName { get; set; }

        public int Flags { get; set; }

        public ulong SourceGuid { get; set; }

        public string SourceName { get; set; }

        public CombatlogEntrySubtype Subtype { get; set; }

        public int TargetFlags { get; set; }

        public DateTime Timestamp { get; set; }

        public CombatlogEntryType Type { get; set; }

        public static bool TryParse(ICombatlogFields fields, List<string> eventArgs, out BasicCombatlogEntry basicCombatLogEntry)
        {
            basicCombatLogEntry = new BasicCombatlogEntry();

            if (eventArgs != null && eventArgs.Count < 8)
            {
                return false;
            }

            basicCombatLogEntry.Args = eventArgs;

            if (double.TryParse(eventArgs[fields.Timestamp].Replace(".", ""), NumberStyles.Any, CultureInfo.InvariantCulture, out double millis))
            {
                basicCombatLogEntry.Timestamp = DateTimeOffset.FromUnixTimeMilliseconds((long)millis).LocalDateTime;
            }
            else
            {
                return false;
            }

            string[] splitted = eventArgs[fields.Type]
                .Replace("SPELL_BUILDING", "SPELLBUILDING")
                .Replace("SPELL_PERIODIC", "SPELLPERIODIC")
                .Split(new char[] { '_' }, 2);

            if (splitted.Length < 2)
            {
                return false;
            }

            if (Enum.TryParse(splitted[0], out CombatlogEntryType type)
                && Enum.TryParse(splitted[1], out CombatlogEntrySubtype subtype))
            {
                basicCombatLogEntry.Type = type;
                basicCombatLogEntry.Subtype = subtype;
            }
            else
            {
                return false;
            }

            if (ulong.TryParse(eventArgs[fields.Source].Replace("0x", ""), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out ulong sourceGuid))
            {
                basicCombatLogEntry.SourceGuid = sourceGuid;
            }
            else
            {
                return false;
            }

            basicCombatLogEntry.SourceName = eventArgs[fields.SourceName];

            if (int.TryParse(eventArgs[fields.Flags], out int flags))
            {
                basicCombatLogEntry.Flags = flags;
            }
            else
            {
                return false;
            }

            if (ulong.TryParse(eventArgs[fields.DestinationGuid].Replace("0x", ""), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out ulong destGuid))
            {
                basicCombatLogEntry.DestinationGuid = destGuid;
            }
            else
            {
                return false;
            }

            basicCombatLogEntry.DestinationName = eventArgs[fields.DestinationName];

            if (int.TryParse(eventArgs[fields.TargetFlags], out int targetFlags))
            {
                basicCombatLogEntry.TargetFlags = targetFlags;
            }
            else
            {
                return false;
            }

            return true;
        }

        public override string ToString()
        {
            return $"{Type}_{Subtype} {SourceName} -> {DestinationName}";
        }
    }
}