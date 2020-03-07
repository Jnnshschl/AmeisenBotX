using AmeisenBotX.Core.Data.CombatLog.Objects;
using AmeisenBotX.Core.Data.Objects.WowObject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace AmeisenBotX.Core.Data.Persistence
{
    [Serializable]
    public class InMemoryBotCache : IAmeisenBotCache
    {
        public InMemoryBotCache(string path)
        {
            FilePath = path;
            Clear();
        }

        public List<BasicCombatLogEntry> CombatLogEntries { get; private set; }

        public string FilePath { get; }

        public Dictionary<ulong, string> NameCache { get; private set; }

        public Dictionary<(int, int), WowUnitReaction> ReactionCache { get; private set; }

        public Dictionary<int, string> SpellNameCache { get; private set; }

        public void CacheName(ulong guid, string name)
        {
            if (!NameCache.ContainsKey(guid))
            {
                NameCache.Add(guid, name);
            }
        }

        public void CacheReaction(int a, int b, WowUnitReaction reaction)
        {
            if (!ReactionCache.ContainsKey((a, b)))
            {
                ReactionCache.Add((a, b), reaction);
            }
        }

        public void CacheSpellName(int spellId, string name)
        {
            if (!SpellNameCache.ContainsKey(spellId))
            {
                SpellNameCache.Add(spellId, name);
            }
        }

        public void Clear()
        {
            NameCache = new Dictionary<ulong, string>();
            ReactionCache = new Dictionary<(int, int), WowUnitReaction>();
            SpellNameCache = new Dictionary<int, string>();
            CombatLogEntries = new List<BasicCombatLogEntry>();
        }

        public void Load()
        {
            if (!Directory.Exists(Path.GetDirectoryName(FilePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
            }

            if (File.Exists(FilePath))
            {
                using Stream stream = File.Open(FilePath, FileMode.Open);
                BinaryFormatter binaryFormatter = new BinaryFormatter();

                try
                {
                    InMemoryBotCache loadedCache = (InMemoryBotCache)binaryFormatter.Deserialize(stream);

                    if (loadedCache != null)
                    {
                        NameCache = loadedCache.NameCache ?? new Dictionary<ulong, string>();
                        ReactionCache = loadedCache.ReactionCache ?? new Dictionary<(int, int), WowUnitReaction>();
                        SpellNameCache = loadedCache.SpellNameCache ?? new Dictionary<int, string>();
                        CombatLogEntries = loadedCache.CombatLogEntries ?? new List<BasicCombatLogEntry>();
                    }
                    else
                    {
                        Clear();
                    }
                }
                catch
                {
                    stream.Close();
                    File.Delete(FilePath);
                    Clear();
                }
            }
        }

        public void Save()
        {
            if (!Directory.Exists(Path.GetDirectoryName(FilePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
            }

            using Stream stream = File.Open(FilePath, FileMode.Create);
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize(stream, this);
        }

        public bool TryGetReaction(int a, int b, out WowUnitReaction reaction)
        {
            if (ReactionCache.ContainsKey((a, b)))
            {
                reaction = ReactionCache[(a, b)];
                return true;
            }
            else if (ReactionCache.ContainsKey((b, a)))
            {
                reaction = ReactionCache[(b, a)];
                return true;
            }

            reaction = WowUnitReaction.Unknown;
            return false;
        }

        public bool TryGetSpellName(int spellId, out string name)
        {
            if (SpellNameCache.ContainsKey(spellId))
            {
                name = SpellNameCache[spellId];
                return true;
            }

            name = "";
            return false;
        }

        public bool TryGetUnitName(ulong guid, out string name)
        {
            if (NameCache.ContainsKey(guid))
            {
                name = NameCache[guid];
                return true;
            }

            name = "";
            return false;
        }
    }
}