using AmeisenBotX.Core.Data.Objects.WowObject;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Data
{
    public class CacheManager
    {
        public Dictionary<ulong, string> NameCache { get; }
        public Dictionary<(int, int), WowUnitReaction> ReactionCache { get; }

        public CacheManager()
        {
            NameCache = new Dictionary<ulong, string>();
            ReactionCache = new Dictionary<(int, int), WowUnitReaction>();
        }
    }
}