using AmeisenBotX.Core.Data.CombatLog.Enums;
using System;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Data.CombatLog.Objects
{
    [Serializable]
    public class BasicCombatLogEntry
    {
        public List<string> Args { get; set; }

        public int DestinationFlags { get; set; }

        public string DestinationGuid { get; set; }

        public string DestinationName { get; set; }

        public int DestinationRaidFlags { get; set; }

        public int Flags { get; set; }

        public string Guid { get; set; }

        public string Name { get; set; }

        public int RaidFlags { get; set; }

        public CombatLogEntrySubtype Subtype { get; set; }

        public string Timestamp { get; set; }

        public CombatLogEntryType Type { get; set; }

        public static bool TryParse(List<string> eventArgs, out BasicCombatLogEntry basicCombatLogEntry)
        {
            basicCombatLogEntry = new BasicCombatLogEntry();

            if (eventArgs.Count < 11)
            {
                return false;
            }

            basicCombatLogEntry.Timestamp = eventArgs[0];

            string[] splitted = eventArgs[1].Split(new char[] { '_' }, 2);

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

            basicCombatLogEntry.Guid = eventArgs[3];
            basicCombatLogEntry.Name = eventArgs[4];

            if (int.TryParse(eventArgs[5], out int flags))
            {
                basicCombatLogEntry.Flags = flags;
            }
            else
            {
                return false;
            }

            if (int.TryParse(eventArgs[6], out int raidFlags))
            {
                basicCombatLogEntry.RaidFlags = raidFlags;
            }
            else
            {
                return false;
            }

            basicCombatLogEntry.DestinationGuid = eventArgs[7];
            basicCombatLogEntry.DestinationName = eventArgs[8];

            if (int.TryParse(eventArgs[9], out int dflags))
            {
                basicCombatLogEntry.DestinationFlags = dflags;
            }
            else
            {
                return false;
            }

            if (int.TryParse(eventArgs[10], out int draidFlags))
            {
                basicCombatLogEntry.DestinationRaidFlags = draidFlags;
            }
            else
            {
                return false;
            }

            return true;
        }
    }
}