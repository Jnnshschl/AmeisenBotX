using AmeisenBotX.Core.Data.Objects.WowObject;

namespace AmeisenBotX.Core.Data.Persistence
{
    public interface IAmeisenBotCache
    {
        void CacheName(ulong guid, string name);

        void CacheReaction(int a, int b, WowUnitReaction reaction);

        void CacheSpellName(int spellId, string name);

        void Clear();

        void Load();

        void Save();

        bool TryGetReaction(int a, int b, out WowUnitReaction reaction);

        bool TryGetSpellName(int spellId, out string name);

        bool TryGetUnitName(ulong guid, out string name);
    }
}