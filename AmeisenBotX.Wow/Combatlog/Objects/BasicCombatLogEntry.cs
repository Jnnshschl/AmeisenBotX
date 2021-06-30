using AmeisenBotX.Wow.Combatlog.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace AmeisenBotX.Wow.Combatlog.Objects
{
    [Serializable]
    public class BasicCombatLogEntry
    {
        public List<string> Args { get; set; }

        public ulong DestinationGuid { get; set; }

        public string DestinationName { get; set; }

        public int Flags { get; set; }

        public ulong SourceGuid { get; set; }

        public string SourceName { get; set; }

        public CombatLogEntrySubtype Subtype { get; set; }

        public int TargetFlags { get; set; }

        public DateTime Timestamp { get; set; }

        public CombatLogEntryType Type { get; set; }

        public static bool TryParse(List<string> eventArgs, out BasicCombatLogEntry basicCombatLogEntry)
        {
            basicCombatLogEntry = new BasicCombatLogEntry();

            if (eventArgs != null && eventArgs.Count < 8)
            {
                return false;
            }

            basicCombatLogEntry.Args = eventArgs;

            if (double.TryParse(eventArgs[0].Replace(".", ""), NumberStyles.Any, CultureInfo.InvariantCulture, out double millis))
            {
                basicCombatLogEntry.Timestamp = DateTimeOffset.FromUnixTimeMilliseconds((long)millis).LocalDateTime;
            }
            else
            {
                return false;
            }

            string[] splitted = eventArgs[1]
                .Replace("SPELL_BUILDING", "SPELLBUILDING")
                .Replace("SPELL_PERIODIC", "SPELLPERIODIC")
                .Split(new char[] { '_' }, 2);

            if (splitted.Length < 2)
            {
                return false;
            }

            if (Enum.TryParse(splitted[0], out CombatLogEntryType type)
                && Enum.TryParse(splitted[1], out CombatLogEntrySubtype subtype))
            {
                basicCombatLogEntry.Type = type;
                basicCombatLogEntry.Subtype = subtype;
            }
            else
            {
                return false;
            }

            if (ulong.TryParse(eventArgs[2].Replace("0x", ""), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out ulong sourceGuid))
            {
                basicCombatLogEntry.SourceGuid = sourceGuid;
            }
            else
            {
                return false;
            }
            basicCombatLogEntry.SourceName = eventArgs[3];

            if (int.TryParse(eventArgs[4], out int flags))
            {
                basicCombatLogEntry.Flags = flags;
            }
            else
            {
                return false;
            }

            if (ulong.TryParse(eventArgs[5].Replace("0x", ""), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out ulong destGuid))
            {
                basicCombatLogEntry.DestinationGuid = destGuid;
            }
            else
            {
                return false;
            }
            basicCombatLogEntry.DestinationName = eventArgs[6];

            if (int.TryParse(eventArgs[7], out int targetFlags))
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