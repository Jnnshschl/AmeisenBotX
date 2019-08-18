using AmeisenBotX.Core.Data.Objects.WowObject;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AmeisenBotX.Core.Data
{
    public class CacheManager
    {
        private AmeisenBotConfig Config { get; }

        public Dictionary<ulong, string> NameCache { get; private set; }
        public Dictionary<(int, int), WowUnitReaction> ReactionCache { get; private set; }

        public CacheManager(AmeisenBotConfig config)
        {
            Config = config;
            NameCache = new Dictionary<ulong, string>();
            ReactionCache = new Dictionary<(int, int), WowUnitReaction>();
        }

        internal void LoadFromFile()
        {
            if (Config.PermanentNameCache
                && File.Exists(Config.PermanentNameCachePath))
                NameCache = JsonConvert.DeserializeObject<Dictionary<ulong, string>>(File.ReadAllText(Config.PermanentNameCachePath));
            if (Config.PermanentReactionCache
                && File.Exists(Config.PermanentReactionCachePath))
                ReactionCache = JsonConvert.DeserializeObject<Dictionary<(int, int), WowUnitReaction>>(File.ReadAllText(Config.PermanentReactionCachePath));
        }

        internal void SaveToFile()
        {
            if (Config.PermanentNameCache)
            {
                if (!Directory.Exists(Path.GetDirectoryName(Config.PermanentNameCachePath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(Config.PermanentNameCachePath));
                File.WriteAllText(Config.PermanentNameCachePath, JsonConvert.SerializeObject(NameCache.OrderBy(e => e.Key), Formatting.Indented));
            }
            if (Config.PermanentReactionCache)
            {
                if (!Directory.Exists(Path.GetDirectoryName(Config.PermanentReactionCachePath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(Config.PermanentReactionCachePath));
                File.WriteAllText(Config.PermanentReactionCachePath, JsonConvert.SerializeObject(ReactionCache.OrderBy(e => e.Key.Item1).ThenBy(e => e.Key.Item2), Formatting.Indented));
            }
        }
    }
}