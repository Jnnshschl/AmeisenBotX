using AmeisenBotX.Core.Data.Objects.WowObject;

namespace AmeisenBotX.Core.Data.Persistence
{
    public interface IAmeisenBotCache
    {
        void Save();

        void Load();

        bool TryGetName(ulong guid, out string name);

        void CacheName(ulong guid, string name);

        bool TryGetReaction(int a, int b, out WowUnitReaction reaction);

        void CacheReaction(int a, int b, WowUnitReaction reaction);
    }
}
