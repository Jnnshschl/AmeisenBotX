﻿using AmeisenBotX.Common.Math;
using AmeisenBotX.Wow.Cache.Enums;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System.Collections.Generic;

namespace AmeisenBotX.Wow.Cache
{
    public interface IAmeisenBotDb
    {
        IReadOnlyDictionary<int, List<Vector3>> AllBlacklistNodes();

        IReadOnlyDictionary<WowMapId, Dictionary<WowHerbId, List<Vector3>>> AllHerbNodes();

        IReadOnlyDictionary<ulong, string> AllNames();

        IReadOnlyDictionary<WowMapId, Dictionary<WowOreId, List<Vector3>>> AllOreNodes();

        IReadOnlyDictionary<WowMapId, Dictionary<PoiType, List<Vector3>>> AllPointsOfInterest();

        IReadOnlyDictionary<int, Dictionary<int, WowUnitReaction>> AllReactions();

        IReadOnlyDictionary<int, string> AllSpellNames();

        void CacheHerb(WowMapId mapId, WowHerbId displayId, Vector3 position);

        void CacheOre(WowMapId mapId, WowOreId displayId, Vector3 position);

        void CachePoi(WowMapId mapId, PoiType poiType, Vector3 position);

        void Clear();

        WowUnitReaction GetReaction(IWowUnit a, IWowUnit b);

        string GetSpellName(int spellId);

        bool GetUnitName(IWowUnit unit, out string name);

        void Save(string dbFile);

        bool TryGetBlacklistPosition(int mapId, Vector3 position, float maxRadius, out IEnumerable<Vector3> nodes);

        bool TryGetPointsOfInterest(WowMapId mapId, PoiType poiType, Vector3 position, float maxRadius, out IEnumerable<Vector3> nodes);
    }
}