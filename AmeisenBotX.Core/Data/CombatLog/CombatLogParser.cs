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
        }

        private WowInterface WowInterface { get; }

        public void Parse(long timestamp, List<string> args)
        {
            AmeisenLogger.Instance.Log("CombatLogParser", $"[{timestamp}] Parsing CombatLog: {JsonConvert.SerializeObject(args)}", LogLevel.Verbose);

            if (BasicCombatLogEntry.TryParse(args, out BasicCombatLogEntry basicCombatLogEntry))
            {
                (CombatLogEntryType, CombatLogEntrySubtype) key = (basicCombatLogEntry.Type, basicCombatLogEntry.Subtype);

                if (!WowInterface.BotCache.CombatLogEntries.ContainsKey(key))
                {
                    WowInterface.BotCache.CombatLogEntries.Add(key, new List<BasicCombatLogEntry>());
                }

                WowInterface.BotCache.CombatLogEntries[key].Add(basicCombatLogEntry);
            }
        }
    }
}