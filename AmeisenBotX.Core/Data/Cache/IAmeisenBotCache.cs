using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Data.Cache
{
    public interface IAmeisenBotCache
    {
        void CacheBlacklistPosition(int mapId, Vector3 node);

        void CacheName(ulong guid, string name);

        void CacheReaction(int a, int b, WowUnitReaction reaction);

        void CacheSpellName(int spellId, string name);

        void Clear();

        void Load();

        void Save();

        bool TryGetBlacklistPosition(int mapId, Vector3 position, double maxRadius, out List<Vector3> nodes);

        bool TryGetReaction(int a, int b, out WowUnitReaction reaction);

        bool TryGetSpellName(int spellId, out string name);

        bool TryGetUnitName(ulong guid, out string name);
    }
}