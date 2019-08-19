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
            {
                dynamic parsed = JsonConvert.DeserializeObject(File.ReadAllText(Path.Combine(Config.BotDataPath, Config.PermanentReactionCachePath)));
                foreach(dynamic item in parsed)
                {
                    NameCache.Add((ulong)item.Key, (string)item.Value);
                }
            }
            if (Config.PermanentReactionCache
                && File.Exists(Config.PermanentReactionCachePath))
            {
                dynamic parsed = JsonConvert.DeserializeObject(File.ReadAllText(Path.Combine(Config.BotDataPath, Config.PermanentReactionCachePath)));
                foreach (dynamic item in parsed)
                {
                    ReactionCache.Add(((int, int))item.Key, (WowUnitReaction)item.Value);
                }
            }
        }

        internal void SaveToFile()
        {
            if (Config.PermanentNameCache)
                CheckForPermanentCaching(Path.Combine(Config.BotDataPath, Config.PermanentReactionCachePath), NameCache.OrderBy(e => e.Key));
            if (Config.PermanentReactionCache)
                CheckForPermanentCaching(Path.Combine(Config.BotDataPath,Config.PermanentReactionCachePath), ReactionCache.OrderBy(e => e.Key.Item1).ThenBy(e => e.Key.Item2));
        }

        private void CheckForPermanentCaching(string path, object objToSave)
        {
            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(path)))
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, JsonConvert.SerializeObject(objToSave, Formatting.Indented));
            }
            catch { }
        }
    }
}