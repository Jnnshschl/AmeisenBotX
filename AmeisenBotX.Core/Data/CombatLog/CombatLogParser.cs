using AmeisenBotX.Core.Data.Persistence;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Data.CombatLog
{
    public class CombatLogParser
    {
        public CombatLogParser(IAmeisenBotCache ameisenBotCache)
        {
            AmeisenBotCache = ameisenBotCache;
        }

        private IAmeisenBotCache AmeisenBotCache { get; }

        public void Parse(long timestamp, List<string> args)
        {
            AmeisenLogger.Instance.Log("CombatLogParser", $"Parsing CombatLog: {JsonConvert.SerializeObject(args)}", LogLevel.Verbose);
        }
    }
}