using AmeisenBotX.Core.Data.CombatLog.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace AmeisenBotX.Core.Data.CombatLog.Objects
{
    [Serializable]
    public class BasicCombatLogEntry
    {
        public List<string> Args { get; set; }

        public string DestinationGuid { get; set; }

        public string DestinationName { get; set; }

        public int Flags { get; set; }

        public string SourceGuid { get; set; }

        public string SourceName { get; set; }

        public CombatLogEntrySubtype Subtype { get; set; }

        public int TargetFlags { get; set; }

        public DateTime Timestamp { get; set; }

        public CombatLogEntryType Type { get; set; }

        public static bool TryParse(List<string> eventArgs, out BasicCombatLogEntry basicCombatLogEntry)
        {
            basicCombatLogEntry = new BasicCombatLogEntry();

            if (eventArgs.Count < 8)
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

            basicCombatLogEntry.SourceGuid = eventArgs[2];
            basicCombatLogEntry.SourceName = eventArgs[3];

            if (int.TryParse(eventArgs[4], out int flags))
            {
                basicCombatLogEntry.Flags = flags;
            }
            else
            {
                return false;
            }

            basicCombatLogEntry.DestinationGuid = eventArgs[5];
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