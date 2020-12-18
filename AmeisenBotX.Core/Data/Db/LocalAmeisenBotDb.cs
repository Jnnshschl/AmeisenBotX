using AmeisenBotX.Core.Data.CombatLog.Enums;
using AmeisenBotX.Core.Data.CombatLog.Objects;
using AmeisenBotX.Core.Data.Db.Enums;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObjects;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Core.Personality.Enums;
using AmeisenBotX.Core.Personality.Objects;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AmeisenBotX.Core.Data.Db
{
    public class LocalAmeisenBotDb : IAmeisenBotDb
    {
        public LocalAmeisenBotDb()
        {
            CombatLogSubject = new BasicCombatLogEntrySubject();
            Clear();
        }

        public ConcurrentDictionary<int, List<Vector3>> BlacklistNodes { get; private set; }

        public ConcurrentDictionary<CombatLogEntryType, Dictionary<CombatLogEntrySubtype, List<BasicCombatLogEntry>>> CombatLogEntries { get; private set; }
        public BasicCombatLogEntrySubject CombatLogSubject { get; }

        public ConcurrentDictionary<MapId, Dictionary<HerbNode, List<Vector3>>> HerbNodes { get; private set; }

        public ConcurrentDictionary<ulong, string> Names { get; private set; }

        public ConcurrentDictionary<MapId, Dictionary<OreNode, List<Vector3>>> OreNodes { get; private set; }

        public ConcurrentDictionary<ulong, Relationship> PlayerRelationships { get; private set; }

        public ConcurrentDictionary<MapId, Dictionary<PoiType, List<Vector3>>> PointsOfInterest { get; private set; }

        public ConcurrentDictionary<int, Dictionary<int, WowUnitReaction>> Reactions { get; private set; }

        public ConcurrentDictionary<int, string> SpellNames { get; private set; }

        public static LocalAmeisenBotDb FromJson(string dbFile)
        {
            if (!Directory.Exists(Path.GetDirectoryName(dbFile)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(dbFile));
            }
            else if (File.Exists(dbFile))
            {
                AmeisenLogger.I.Log("Cache", "Loading Cache", LogLevel.Debug);

                try
                {
                    return JsonConvert.DeserializeObject<LocalAmeisenBotDb>(File.ReadAllText(dbFile));
                }
                catch (Exception ex)
                {
                    AmeisenLogger.I.Log("Cache", $"Error while loading db:\n{ex}", LogLevel.Debug);
                }
            }

            LocalAmeisenBotDb db = new LocalAmeisenBotDb();
            db.Clear();

            return db;
        }

        public void AddPlayerRelationship(WowPlayer player, RelationshipLevel initialRelationship = RelationshipLevel.Neutral)
        {
            if (!IsPlayerKnown(player))
            {
                PlayerRelationships.TryAdd(player.Guid, new Relationship()
                {
                    Score = (float)initialRelationship,
                    FirstSeen = DateTime.Now,
                    FirstSeenMapId = WowInterface.I.ObjectManager.MapId,
                    FirstSeenPosition = player.Position
                });
            }

            PlayerRelationships[player.Guid].Poll(player);
        }

        public IReadOnlyDictionary<int, List<Vector3>> AllBlacklistNodes()
        {
            return BlacklistNodes;
        }

        public IReadOnlyDictionary<CombatLogEntryType, Dictionary<CombatLogEntrySubtype, List<BasicCombatLogEntry>>> AllCombatLogEntries()
        {
            return CombatLogEntries;
        }

        public IReadOnlyDictionary<MapId, Dictionary<HerbNode, List<Vector3>>> AllHerbNodes()
        {
            return HerbNodes;
        }

        public IReadOnlyDictionary<ulong, string> AllNames()
        {
            return Names;
        }

        public IReadOnlyDictionary<MapId, Dictionary<OreNode, List<Vector3>>> AllOreNodes()
        {
            return OreNodes;
        }

        public IReadOnlyDictionary<ulong, Relationship> AllPlayerRelationships()
        {
            return PlayerRelationships;
        }

        public IReadOnlyDictionary<MapId, Dictionary<PoiType, List<Vector3>>> AllPointsOfInterest()
        {
            return PointsOfInterest;
        }

        public IReadOnlyDictionary<int, Dictionary<int, WowUnitReaction>> AllReactions()
        {
            return Reactions;
        }

        public IReadOnlyDictionary<int, string> AllSpellNames()
        {
            return SpellNames;
        }

        public void CacheBlacklistPosition(int mapId, Vector3 node)
        {
            if (!TryGetBlacklistPosition(mapId, node, 8, out _))
            {
                if (!BlacklistNodes.ContainsKey(mapId))
                {
                    BlacklistNodes.TryAdd(mapId, new List<Vector3>() { node });
                }
                else if (!BlacklistNodes[mapId].Contains(node))
                {
                    BlacklistNodes[mapId].Add(node);
                }
            }
        }

        public void CacheCombatLogEntry(KeyValuePair<CombatLogEntryType, CombatLogEntrySubtype> key, BasicCombatLogEntry entry)
        {
            if (!CombatLogEntries.ContainsKey(key.Key))
            {
                CombatLogEntries.TryAdd(key.Key, new Dictionary<CombatLogEntrySubtype, List<BasicCombatLogEntry>>() { { key.Value, new List<BasicCombatLogEntry>() } });
            }
            else if (!CombatLogEntries[key.Key].ContainsKey(key.Value))
            {
                CombatLogEntries[key.Key].Add(key.Value, new List<BasicCombatLogEntry>() { entry });
            }
            else
            {
                CombatLogEntries[key.Key][key.Value].Add(entry);
            }
        }

        public void CacheHerb(MapId mapId, HerbNode displayId, Vector3 position)
        {
            if (!HerbNodes.ContainsKey(mapId))
            {
                HerbNodes.TryAdd(mapId, new Dictionary<HerbNode, List<Vector3>>() { { displayId, new List<Vector3>() { position } } });
            }
            else if (!HerbNodes[mapId].ContainsKey(displayId))
            {
                HerbNodes[mapId].Add(displayId, new List<Vector3>() { position });
            }
            else if (!HerbNodes[mapId][displayId].Any(e => e == position))
            {
                HerbNodes[mapId][displayId].Add(position);
            }
        }

        public void CacheName(ulong guid, string name)
        {
            if (!Names.ContainsKey(guid))
            {
                Names.TryAdd(guid, name);
            }
            else
            {
                Names[guid] = name;
            }
        }

        public void CacheOre(MapId mapId, OreNode displayId, Vector3 position)
        {
            if (!OreNodes.ContainsKey(mapId))
            {
                OreNodes.TryAdd(mapId, new Dictionary<OreNode, List<Vector3>>() { { displayId, new List<Vector3>() { position } } });
            }
            else if (!OreNodes[mapId].ContainsKey(displayId))
            {
                OreNodes[mapId].Add(displayId, new List<Vector3>() { position });
            }
            else if (!OreNodes[mapId][displayId].Any(e => e == position))
            {
                OreNodes[mapId][displayId].Add(position);
            }
        }

        public void CachePoi(MapId mapId, PoiType poiType, Vector3 position)
        {
            if (!PointsOfInterest.ContainsKey(mapId))
            {
                PointsOfInterest.TryAdd(mapId, new Dictionary<PoiType, List<Vector3>>() { { poiType, new List<Vector3>() { position } } });
            }
            else if (!PointsOfInterest[mapId].ContainsKey(poiType))
            {
                PointsOfInterest[mapId].Add(poiType, new List<Vector3>() { position });
            }
            else if (!PointsOfInterest[mapId][poiType].Any(e => e == position))
            {
                PointsOfInterest[mapId][poiType].Add(position);
            }
        }

        public void CacheReaction(int a, int b, WowUnitReaction reaction)
        {
            if (!Reactions.ContainsKey(a))
            {
                Reactions.TryAdd(a, new Dictionary<int, WowUnitReaction>() { { b, reaction } });
            }
            else if (!Reactions[a].ContainsKey(b))
            {
                Reactions[a].Add(b, reaction);
            }
            else
            {
                Reactions[a][b] = reaction;
            }
        }

        public void CacheSpellName(int spellId, string name)
        {
            if (!SpellNames.ContainsKey(spellId))
            {
                SpellNames.TryAdd(spellId, name);
            }
        }

        public void Clear()
        {
            Names = new ConcurrentDictionary<ulong, string>();
            Reactions = new ConcurrentDictionary<int, Dictionary<int, WowUnitReaction>>();
            SpellNames = new ConcurrentDictionary<int, string>();
            CombatLogEntries = new ConcurrentDictionary<CombatLogEntryType, Dictionary<CombatLogEntrySubtype, List<BasicCombatLogEntry>>>();
            BlacklistNodes = new ConcurrentDictionary<int, List<Vector3>>();
            PointsOfInterest = new ConcurrentDictionary<MapId, Dictionary<PoiType, List<Vector3>>>();
            OreNodes = new ConcurrentDictionary<MapId, Dictionary<OreNode, List<Vector3>>>();
            HerbNodes = new ConcurrentDictionary<MapId, Dictionary<HerbNode, List<Vector3>>>();
            PlayerRelationships = new ConcurrentDictionary<ulong, Relationship>();
        }

        public bool IsPlayerKnown(WowPlayer player)
        {
            return PlayerRelationships.ContainsKey(player.Guid);
        }

        public void Save(string dbFile)
        {
            if (!Directory.Exists(Path.GetDirectoryName(dbFile)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(dbFile));
            }

            File.WriteAllText(dbFile, JsonConvert.SerializeObject(this));
        }

        public bool TryGetBlacklistPosition(int mapId, Vector3 position, double maxRadius, out IEnumerable<Vector3> nodes)
        {
            if (BlacklistNodes.ContainsKey(mapId))
            {
                nodes = BlacklistNodes[mapId].Where(e => e.GetDistance(position) < maxRadius);
                return nodes.Any();
            }

            nodes = null;
            return false;
        }

        public bool TryGetPlayerRelationship(WowPlayer player, out Relationship relationship)
        {
            if (PlayerRelationships.ContainsKey(player.Guid))
            {
                relationship = PlayerRelationships[player.Guid];
                return true;
            }

            relationship = default;
            return false;
        }

        public bool TryGetPointsOfInterest(MapId mapId, PoiType poiType, Vector3 position, double maxRadius, out IEnumerable<Vector3> nodes)
        {
            KeyValuePair<MapId, PoiType> KeyValuePair = new KeyValuePair<MapId, PoiType>(mapId, poiType);

            if (PointsOfInterest.ContainsKey(mapId)
                && PointsOfInterest[mapId].ContainsKey(poiType))
            {
                nodes = PointsOfInterest[mapId][poiType].Where(e => e.GetDistance(position) < maxRadius);
                return nodes.Any();
            }

            nodes = null;
            return false;
        }

        public bool TryGetReaction(int a, int b, out WowUnitReaction reaction)
        {
            if (Reactions.ContainsKey(a)
                && Reactions[a].ContainsKey(b))
            {
                reaction = Reactions[a][b];
                return true;
            }

            reaction = WowUnitReaction.Unknown;
            return false;
        }

        public bool TryGetSpellName(int spellId, out string name)
        {
            if (SpellNames.ContainsKey(spellId))
            {
                name = SpellNames[spellId];
                return true;
            }

            name = string.Empty;
            return false;
        }

        public bool TryGetUnitName(ulong guid, out string name)
        {
            if (Names.ContainsKey(guid))
            {
                name = Names[guid];
                return true;
            }

            name = string.Empty;
            return false;
        }

        public void UpdatePlayerRelationship(WowPlayer player)
        {
            if (IsPlayerKnown(player))
            {
                PlayerRelationships[player.Guid].Poll(player);
            }
        }

        public BasicCombatLogEntrySubject GetCombatLogSubject()
        {
            return CombatLogSubject;
        }
    }
}