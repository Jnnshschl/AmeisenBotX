using AmeisenBotX.Core.Data.Objects.WowObject;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AmeisenBotX.Core.Data
{
    public class CacheManager
    {
        public CacheManager(string botDataPath, string playername, AmeisenBotConfig config)
        {
            BotDataPath = botDataPath;
            PlayerName = playername;
            Config = config;
            NameCache = new Dictionary<ulong, string>();
            ReactionCache = new Dictionary<(int, int), WowUnitReaction>();
        }

        public string PlayerName { get; }

        public Dictionary<ulong, string> NameCache { get; private set; }

        public Dictionary<(int, int), WowUnitReaction> ReactionCache { get; private set; }

        private AmeisenBotConfig Config { get; }

        private string BotDataPath { get; }

        internal void LoadFromFile()
        {
            if (Config.PermanentNameCache
                && File.Exists(Path.Combine(BotDataPath, PlayerName, "name_cache.json")))
            {
                dynamic parsed = JsonConvert.DeserializeObject(File.ReadAllText(Path.Combine(BotDataPath, PlayerName, "name_cache.json")));
                foreach (dynamic item in parsed)
                {
                    NameCache.Add((ulong)item.Key, (string)item.Value);
                }
            }

            if (Config.PermanentReactionCache
                && File.Exists(Path.Combine(BotDataPath, PlayerName, "reaction_cache.json")))
            {
                dynamic parsed = JsonConvert.DeserializeObject(File.ReadAllText(Path.Combine(BotDataPath, PlayerName, "reaction_cache.json")));
                foreach (dynamic item in parsed)
                {
                    ReactionCache.Add(((int, int))item.Key, (WowUnitReaction)item.Value);
                }
            }
        }

        internal void SaveToFile(string characterName)
        {
            if (Config.PermanentNameCache)
            {
                CheckForPermanentCaching(Path.Combine(BotDataPath, PlayerName, "name_cache.json"), NameCache.OrderBy(e => e.Key));
            }

            if (Config.PermanentReactionCache)
            {
                CheckForPermanentCaching(Path.Combine(BotDataPath, PlayerName, "reaction_cache.json"), ReactionCache.OrderBy(e => e.Key.Item1).ThenBy(e => e.Key.Item2));
            }
        }

        private void CheckForPermanentCaching(string path, object objToSave)
        {
            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(path)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                }

                File.WriteAllText(path, JsonConvert.SerializeObject(objToSave, Formatting.Indented));
            }
            catch
            {
            }
        }
    }
}