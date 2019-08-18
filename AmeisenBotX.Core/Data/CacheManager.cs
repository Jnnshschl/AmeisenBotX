using AmeisenBotX.Core.Data.Objects.WowObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.Data
{
    public class CacheManager
    {
        public Dictionary<ulong, string> NameCache { get; }
        public Dictionary<(int,int), WowUnitReaction> ReactionCache { get; }

        public CacheManager()
        {
            NameCache = new Dictionary<ulong, string>();
            ReactionCache = new Dictionary<(int, int), WowUnitReaction>();
        }
    }
}
