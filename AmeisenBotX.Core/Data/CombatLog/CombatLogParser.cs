using AmeisenBotX.Core.Data.CombatLog.Enums;
using AmeisenBotX.Core.Data.CombatLog.Objects;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Data.CombatLog
{
    public class CombatLogParser
    {
        public CombatLogParser(WowInterface wowInterface)
        {
            WowInterface = wowInterface;

            CombatLogEntries = new Dictionary<(CombatLogEntryType, CombatLogEntrySubtype), List<BasicCombatLogEntry>>();
        }

        private WowInterface WowInterface { get; }

        public Dictionary<(CombatLogEntryType, CombatLogEntrySubtype), List<BasicCombatLogEntry>> CombatLogEntries { get; }

        public void Parse(long timestamp, List<string> args)
        {
            AmeisenLogger.Instance.Log("CombatLogParser", $"[{timestamp}] Parsing CombatLog: {JsonConvert.SerializeObject(args)}", LogLevel.Verbose);

            if (BasicCombatLogEntry.TryParse(args, out BasicCombatLogEntry basicCombatLogEntry))
            {
                (CombatLogEntryType, CombatLogEntrySubtype) key = (basicCombatLogEntry.Type, basicCombatLogEntry.Subtype);

                if (CombatLogEntries.ContainsKey(key))
                {
                    CombatLogEntries.Add(key, new List<BasicCombatLogEntry>());
                }

                CombatLogEntries[key].Add(basicCombatLogEntry);
            }
        }
    }
}