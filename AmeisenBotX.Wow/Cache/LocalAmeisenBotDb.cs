using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Offsets;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using AmeisenBotX.Memory;
using AmeisenBotX.Wow.Cache.Enums;
using AmeisenBotX.Wow.Combatlog.Enums;
using AmeisenBotX.Wow.Combatlog.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace AmeisenBotX.Wow.Cache
{
    public class LocalAmeisenBotDb : IAmeisenBotDb
    {
        public LocalAmeisenBotDb()
        {
            CombatLogSubject = new BasicCombatLogEntrySubject();
            Clear();

            CleanupTimer = new Timer(CleanupTimerTick, null, 0, 6000);
        }

        public ConcurrentDictionary<int, List<Vector3>> BlacklistNodes { get; private set; }

        public ConcurrentDictionary<CombatLogEntryType, Dictionary<CombatLogEntrySubtype, List<(DateTime, BasicCombatLogEntry)>>> CombatLogEntries { get; private set; }

        public BasicCombatLogEntrySubject CombatLogSubject { get; }

        public ConcurrentDictionary<WowMapId, Dictionary<WowHerbId, List<Vector3>>> HerbNodes { get; private set; }

        public ConcurrentDictionary<ulong, string> Names { get; private set; }

        public ConcurrentDictionary<WowMapId, Dictionary<WowOreId, List<Vector3>>> OreNodes { get; private set; }

        public ConcurrentDictionary<WowMapId, Dictionary<PoiType, List<Vector3>>> PointsOfInterest { get; private set; }

        public ConcurrentDictionary<int, Dictionary<int, WowUnitReaction>> Reactions { get; private set; }

        public ConcurrentDictionary<int, string> SpellNames { get; private set; }

        private Timer CleanupTimer { get; }

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

            return new();
        }

        public void CacheCombatLogEntry(KeyValuePair<CombatLogEntryType, CombatLogEntrySubtype> key, BasicCombatLogEntry entry)
        {
            if (!CombatLogEntries.ContainsKey(key.Key))
            {
                CombatLogEntries.TryAdd(key.Key, new Dictionary<CombatLogEntrySubtype, List<(DateTime, BasicCombatLogEntry)>>() { { key.Value, new List<(DateTime, BasicCombatLogEntry)>() } });
            }
            else if (!CombatLogEntries[key.Key].ContainsKey(key.Value))
            {
                CombatLogEntries[key.Key].Add(key.Value, new List<(DateTime, BasicCombatLogEntry)>() { (DateTime.UtcNow, entry) });
            }
            else
            {
                CombatLogEntries[key.Key][key.Value].Add((DateTime.UtcNow, entry));
            }
        }

        public void CacheHerb(WowMapId mapId, WowHerbId displayId, Vector3 position)
        {
            if (!HerbNodes.ContainsKey(mapId))
            {
                HerbNodes.TryAdd(mapId, new Dictionary<WowHerbId, List<Vector3>>() { { displayId, new List<Vector3>() { position } } });
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

        public void CacheOre(WowMapId mapId, WowOreId displayId, Vector3 position)
        {
            if (!OreNodes.ContainsKey(mapId))
            {
                OreNodes.TryAdd(mapId, new Dictionary<WowOreId, List<Vector3>>() { { displayId, new List<Vector3>() { position } } });
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

        public void CachePoi(WowMapId mapId, PoiType poiType, Vector3 position)
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
            CombatLogEntries = new ConcurrentDictionary<CombatLogEntryType, Dictionary<CombatLogEntrySubtype, List<(DateTime, BasicCombatLogEntry)>>>();
            BlacklistNodes = new ConcurrentDictionary<int, List<Vector3>>();
            PointsOfInterest = new ConcurrentDictionary<WowMapId, Dictionary<PoiType, List<Vector3>>>();
            OreNodes = new ConcurrentDictionary<WowMapId, Dictionary<WowOreId, List<Vector3>>>();
            HerbNodes = new ConcurrentDictionary<WowMapId, Dictionary<WowHerbId, List<Vector3>>>();
        }

        public BasicCombatLogEntrySubject GetCombatLogSubject()
        {
            return CombatLogSubject;
        }

        public WowUnitReaction GetReaction(Func<IntPtr, IntPtr, WowUnitReaction> pred, WowUnit a, WowUnit b)
        {
            if (Reactions.ContainsKey(a.FactionTemplate) && Reactions[a.FactionTemplate].ContainsKey(b.FactionTemplate))
            {
                return Reactions[a.FactionTemplate][b.FactionTemplate];
            }
            else
            {
                WowUnitReaction reaction = pred(a.BaseAddress, b.BaseAddress);
                CacheReaction(a.FactionTemplate, b.FactionTemplate, reaction);
                return reaction;
            }
        }

        public bool GetUnitName(XMemory xMemory, IOffsetList offsetList, WowUnit unit, out string name)
        {
            if (Names.ContainsKey(unit.Guid))
            {
                name = Names[unit.Guid];
                return true;
            }
            else
            {
                name = unit.ReadName(xMemory, offsetList);
                Names.TryAdd(unit.Guid, name);
                return true;
            }
        }

        public void Save(string dbFile)
        {
            if (!Directory.Exists(Path.GetDirectoryName(dbFile)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(dbFile));
            }

            try
            {
                File.WriteAllText(dbFile, JsonConvert.SerializeObject(this));
            }
            catch
            {
                File.Delete(dbFile);
            }
        }

        public bool TryGetBlacklistPosition(int mapId, Vector3 position, float maxRadius, out IEnumerable<Vector3> nodes)
        {
            if (BlacklistNodes.ContainsKey(mapId))
            {
                nodes = BlacklistNodes[mapId].Where(e => e.GetDistance(position) < maxRadius);
                return nodes.Any();
            }

            nodes = null;
            return false;
        }

        public bool TryGetPointsOfInterest(WowMapId mapId, PoiType poiType, Vector3 position, float maxRadius, out IEnumerable<Vector3> nodes)
        {
            KeyValuePair<WowMapId, PoiType> KeyValuePair = new KeyValuePair<WowMapId, PoiType>(mapId, poiType);

            if (PointsOfInterest.ContainsKey(mapId)
                && PointsOfInterest[mapId].ContainsKey(poiType))
            {
                nodes = PointsOfInterest[mapId][poiType].Where(e => e.GetDistance(position) < maxRadius);
                return nodes.Any();
            }

            nodes = null;
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

        private void CleanupTimerTick(object state)
        {
            if (CombatLogEntries != null && CombatLogEntries.Any())
            {
                DateTime ts = DateTime.UtcNow - TimeSpan.FromMinutes(6);

                foreach (Dictionary<CombatLogEntrySubtype, List<(DateTime, BasicCombatLogEntry)>> e in CombatLogEntries.Values)
                {
                    foreach (List<(DateTime, BasicCombatLogEntry)> x in e.Values)
                    {
                        x.RemoveAll(y => y.Item1 < ts);
                    }
                }
            }
        }
    }
}