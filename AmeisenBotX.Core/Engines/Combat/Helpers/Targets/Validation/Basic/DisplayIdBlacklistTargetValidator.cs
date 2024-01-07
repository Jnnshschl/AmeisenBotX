using AmeisenBotX.Wow.Objects;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Combat.Helpers.Targets.Validation.Basic
{
    public class DisplayIdBlacklistTargetValidator(IEnumerable<int> blacklistedGuids = null) : ITargetValidator
    {
        public IEnumerable<int> Blacklist { get; set; } = blacklistedGuids ?? new List<int>();

        public bool IsValid(IWowUnit unit)
        {
            return !Blacklist.Any(e => e == unit.DisplayId);
        }
    }
}