using AmeisenBotX.Core.Data.CombatLog.Enums;
using AmeisenBotX.Core.Data.CombatLog.Objects;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;

namespace AmeisenBotX.Core.Data.CombatLog
{
    public class CombatLogParser
    {
        public CombatLogParser(WowInterface wowInterface)
        {
            WowInterface = wowInterface;
        }

        private WowInterface WowInterface { get; }

        public void Parse(long timestamp, List<string> args)
        {
            AmeisenLogger.I.Log("CombatLogParser", $"[{timestamp}] Parsing CombatLog: {JsonConvert.SerializeObject(args)}", LogLevel.Verbose);

            if (BasicCombatLogEntry.TryParse(args, out BasicCombatLogEntry basicCombatLogEntry))
            {
                KeyValuePair<CombatLogEntryType, CombatLogEntrySubtype> key = new KeyValuePair<CombatLogEntryType, CombatLogEntrySubtype>(basicCombatLogEntry.Type, basicCombatLogEntry.Subtype);
                WowInterface.Db.CacheCombatLogEntry(key, basicCombatLogEntry);
                WowInterface.Db.GetCombatLogSubject().Next(basicCombatLogEntry);
            }
        }
    }
}