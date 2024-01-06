using AmeisenBotX.Wow.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Combat.Helpers.Targets.Validation.Util
{
    public class CachedTargetValidator : ITargetValidator
    {
        public CachedTargetValidator(ITargetValidator validator, TimeSpan maxCacheTime)
        {
            Validators = [validator];
            Cache = [];
            MaxCacheTime = maxCacheTime;
        }

        public CachedTargetValidator(IEnumerable<ITargetValidator> validators, TimeSpan maxCacheTime)
        {
            Validators = new(validators);
            Cache = [];
            MaxCacheTime = maxCacheTime;
        }

        public TimeSpan MaxCacheTime { get; }

        public List<ITargetValidator> Validators { get; }

        private Dictionary<ulong, (DateTime, bool)> Cache { get; }

        public bool IsValid(IWowUnit unit)
        {
            if (Cache.ContainsKey(unit.Guid))
            {
                (DateTime, bool) cachedEntry = Cache[unit.Guid];

                if (DateTime.UtcNow - cachedEntry.Item1 < MaxCacheTime)
                {
                    return cachedEntry.Item2;
                }
                else
                {
                    Cache.Remove(unit.Guid);
                }
            }

            bool isValid = Validators.All(e => e.IsValid(unit));
            Cache.Add(unit.Guid, (DateTime.UtcNow, isValid));
            return isValid;
        }
    }
}