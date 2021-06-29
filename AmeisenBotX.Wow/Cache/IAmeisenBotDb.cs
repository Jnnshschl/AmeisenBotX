using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Offsets;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Memory;
using AmeisenBotX.Wow.Cache.Enums;
using AmeisenBotX.Wow.Combatlog.Enums;
using AmeisenBotX.Wow.Combatlog.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Collections.Generic;

namespace AmeisenBotX.Wow.Cache
{
    public interface IAmeisenBotDb
    {
        void CacheCombatLogEntry(KeyValuePair<CombatLogEntryType, CombatLogEntrySubtype> key, BasicCombatLogEntry entry);

        void CacheHerb(WowMapId mapId, WowHerbId displayId, Vector3 position);

        void CacheOre(WowMapId mapId, WowOreId displayId, Vector3 position);

        void CachePoi(WowMapId mapId, PoiType poiType, Vector3 position);

        void CacheSpellName(int spellId, string name);

        void Clear();

        BasicCombatLogEntrySubject GetCombatLogSubject();

        WowUnitReaction GetReaction(Func<IntPtr, IntPtr, WowUnitReaction> pred, WowUnit a, WowUnit b);

        bool GetUnitName(XMemory xMemory, IOffsetList offsetList, WowUnit unit, out string name);

        void Save(string dbFile);

        bool TryGetBlacklistPosition(int mapId, Vector3 position, float maxRadius, out IEnumerable<Vector3> nodes);

        bool TryGetPointsOfInterest(WowMapId mapId, PoiType poiType, Vector3 position, float maxRadius, out IEnumerable<Vector3> nodes);

        bool TryGetSpellName(int spellId, out string name);
    }
}