using AmeisenBotX.Core.Data.Objects.WowObject;

namespace AmeisenBotX.Core.Data.Persistence
{
    public interface IAmeisenBotCache
    {
        void CacheName(ulong guid, string name);

        void CacheReaction(int a, int b, WowUnitReaction reaction);

        void Clear();

        void Load();

        void Save();

        bool TryGetName(ulong guid, out string name);

        bool TryGetReaction(int a, int b, out WowUnitReaction reaction);
    }
}
