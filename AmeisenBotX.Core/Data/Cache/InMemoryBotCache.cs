using AmeisenBotX.Core.Data.Cache.Enums;
using AmeisenBotX.Core.Data.CombatLog.Objects;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace AmeisenBotX.Core.Data.Cache
{
    [Serializable]
    public class InMemoryBotCache : IAmeisenBotCache
    {
        public InMemoryBotCache(string path)
        {
            FilePath = path;
            Clear();
        }

        public Dictionary<int, List<Vector3>> BlacklistNodes { get; private set; }

        public List<BasicCombatLogEntry> CombatLogEntries { get; private set; }

        public string FilePath { get; }

        public Dictionary<(MapId, HerbNodes), List<Vector3>> HerbNodes { get; private set; }

        public Dictionary<ulong, string> NameCache { get; private set; }

        public Dictionary<(MapId, OreNodes), List<Vector3>> OreNodes { get; private set; }

        public Dictionary<(MapId, PoiType), List<Vector3>> PointsOfInterest { get; private set; }

        public Dictionary<(int, int), WowUnitReaction> ReactionCache { get; private set; }

        public Dictionary<int, string> SpellNameCache { get; private set; }

        public void CacheBlacklistPosition(int mapId, Vector3 node)
        {
            if (!TryGetBlacklistPosition(mapId, node, 8, out List<Vector3> nodes)
                || !nodes.Any())
            {
                if (!BlacklistNodes.ContainsKey(mapId))
                {
                    BlacklistNodes.Add(mapId, new List<Vector3>() { node });
                }
                else if (!BlacklistNodes[mapId].Contains(node))
                {
                    BlacklistNodes[mapId].Add(node);
                }
            }
        }

        public void CacheHerb(MapId mapId, HerbNodes displayId, Vector3 position)
        {
            if (!HerbNodes.ContainsKey((mapId, displayId)))
            {
                HerbNodes.Add((mapId, displayId), new List<Vector3>() { position });
            }
            else if (!HerbNodes[(mapId, displayId)].Any(e => e == position))
            {
                HerbNodes[(mapId, displayId)].Add(position);
            }
        }

        public void CacheName(ulong guid, string name)
        {
            if (!NameCache.ContainsKey(guid))
            {
                NameCache.Add(guid, name);
            }
        }

        public void CacheOre(MapId mapId, OreNodes displayId, Vector3 position)
        {
            if (!OreNodes.ContainsKey((mapId, displayId)))
            {
                OreNodes.Add((mapId, displayId), new List<Vector3>() { position });
            }
            else if (!OreNodes[(mapId, displayId)].Any(e => e == position))
            {
                OreNodes[(mapId, displayId)].Add(position);
            }
        }

        public void CachePoi(MapId mapId, PoiType poiType, Vector3 position)
        {
            if (!PointsOfInterest.ContainsKey((mapId, poiType)))
            {
                PointsOfInterest.Add((mapId, poiType), new List<Vector3>() { position });
            }
            else if (!PointsOfInterest[(mapId, poiType)].Any(e => e == position))
            {
                PointsOfInterest[(mapId, poiType)].Add(position);
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
            BlacklistNodes = new Dictionary<int, List<Vector3>>();
            PointsOfInterest = new Dictionary<(MapId, PoiType), List<Vector3>>();
            OreNodes = new Dictionary<(MapId, OreNodes), List<Vector3>>();
            HerbNodes = new Dictionary<(MapId, HerbNodes), List<Vector3>>();
        }

        public void Load()
        {
            if (!Directory.Exists(Path.GetDirectoryName(FilePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
            }

            if (File.Exists(FilePath))
            {
                AmeisenLogger.Instance.Log("Cache", "Loading Cache", LogLevel.Debug);
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
                        BlacklistNodes = loadedCache.BlacklistNodes ?? new Dictionary<int, List<Vector3>>();
                        PointsOfInterest = loadedCache.PointsOfInterest ?? new Dictionary<(MapId, PoiType), List<Vector3>>();
                        OreNodes = loadedCache.OreNodes ?? new Dictionary<(MapId, OreNodes), List<Vector3>>();
                        HerbNodes = loadedCache.HerbNodes ?? new Dictionary<(MapId, HerbNodes), List<Vector3>>();
                    }
                    else
                    {
                        Clear();
                    }
                }
                catch
                {
                    AmeisenLogger.Instance.Log("Cache", "Deleting invalid cache", LogLevel.Debug);
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

        public bool TryGetBlacklistPosition(int mapId, Vector3 position, double maxRadius, out List<Vector3> nodes)
        {
            if (BlacklistNodes.ContainsKey(mapId))
            {
                nodes = BlacklistNodes[mapId].Where(e => e.GetDistance(position) < maxRadius).ToList();
                return true;
            }

            nodes = new List<Vector3>();
            return false;
        }

        public bool TryGetPointsOfInterest(MapId mapId, PoiType poiType, Vector3 position, double maxRadius, out List<Vector3> nodes)
        {
            if (PointsOfInterest.ContainsKey((mapId, poiType)))
            {
                nodes = PointsOfInterest[(mapId, poiType)].Where(e => e.GetDistance(position) < maxRadius).ToList();
                return true;
            }

            nodes = new List<Vector3>();
            return false;
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