using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using AmeisenBotX.Wow.Cache;
using AmeisenBotX.Wow.Combatlog.Enums;
using AmeisenBotX.Wow.Combatlog.Objects;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace AmeisenBotX.Wow.Combatlog
{
    public class DefaultCombatLogParser : ICombatLogParser
    {
        public DefaultCombatLogParser(IAmeisenBotDb db)
        {
            Db = db;
        }

        private IAmeisenBotDb Db { get; }

        public void Parse(long timestamp, List<string> args)
        {
            AmeisenLogger.I.Log("CombatLogParser", $"[{timestamp}] Parsing CombatLog: {JsonConvert.SerializeObject(args)}", LogLevel.Verbose);

            if (BasicCombatLogEntry.TryParse(args, out BasicCombatLogEntry basicCombatLogEntry))
            {
                KeyValuePair<CombatLogEntryType, CombatLogEntrySubtype> key = new(basicCombatLogEntry.Type, basicCombatLogEntry.Subtype);
                Db.CacheCombatLogEntry(key, basicCombatLogEntry);
                Db.GetCombatLogSubject().Next(basicCombatLogEntry);
            }
        }
    }
}