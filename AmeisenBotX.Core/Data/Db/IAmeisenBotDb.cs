using AmeisenBotX.Core.Data.CombatLog.Enums;
using AmeisenBotX.Core.Data.CombatLog.Objects;
using AmeisenBotX.Core.Data.Db.Enums;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Core.Personality.Enums;
using AmeisenBotX.Core.Personality.Objects;
using System;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Data.Db
{
    public interface IAmeisenBotDb
    {
        void AddPlayerRelationship(WowPlayer player, RelationshipLevel initialRelationship = RelationshipLevel.Neutral);

        IReadOnlyDictionary<int, List<Vector3>> AllBlacklistNodes();

        IReadOnlyDictionary<CombatLogEntryType, Dictionary<CombatLogEntrySubtype, List<(DateTime, BasicCombatLogEntry)>>> AllCombatLogEntries();

        IReadOnlyDictionary<WowMapId, Dictionary<WowHerbId, List<Vector3>>> AllHerbNodes();

        IReadOnlyDictionary<ulong, string> AllNames();

        IReadOnlyDictionary<WowMapId, Dictionary<WowOreId, List<Vector3>>> AllOreNodes();

        IReadOnlyDictionary<ulong, Relationship> AllPlayerRelationships();

        IReadOnlyDictionary<WowMapId, Dictionary<PoiType, List<Vector3>>> AllPointsOfInterest();

        IReadOnlyDictionary<int, Dictionary<int, WowUnitReaction>> AllReactions();

        IReadOnlyDictionary<int, string> AllSpellNames();

        void CacheBlacklistPosition(int mapId, Vector3 node);

        void CacheCombatLogEntry(KeyValuePair<CombatLogEntryType, CombatLogEntrySubtype> key, BasicCombatLogEntry entry);

        void CacheHerb(WowMapId mapId, WowHerbId displayId, Vector3 position);

        void CacheName(ulong guid, string name);

        void CacheOre(WowMapId mapId, WowOreId displayId, Vector3 position);

        void CachePoi(WowMapId mapId, PoiType poiType, Vector3 position);

        void CacheReaction(int a, int b, WowUnitReaction reaction);

        void CacheSpellName(int spellId, string name);

        void Clear();

        BasicCombatLogEntrySubject GetCombatLogSubject();

        bool IsPlayerKnown(WowPlayer player);

        void Save(string dbFile);

        bool TryGetBlacklistPosition(int mapId, Vector3 position, float maxRadius, out IEnumerable<Vector3> nodes);

        bool TryGetPlayerRelationship(WowPlayer player, out Relationship relationship);

        bool TryGetPointsOfInterest(WowMapId mapId, PoiType poiType, Vector3 position, float maxRadius, out IEnumerable<Vector3> nodes);

        bool TryGetReaction(int a, int b, out WowUnitReaction reaction);

        bool TryGetSpellName(int spellId, out string name);

        bool TryGetUnitName(ulong guid, out string name);

        void UpdatePlayerRelationship(WowPlayer player);
    }
}