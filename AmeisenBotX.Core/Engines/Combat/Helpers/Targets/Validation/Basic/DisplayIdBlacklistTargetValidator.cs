using AmeisenBotX.Wow.Objects;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Combat.Helpers.Targets.Validation.Basic
{
    public class DisplayIdBlacklistTargetValidator : ITargetValidator
    {
        public DisplayIdBlacklistTargetValidator(IEnumerable<int> blacklistedGuids = null)
        {
            Blacklist = blacklistedGuids ?? new List<int>();
        }

        public IEnumerable<int> Blacklist { get; set; }

        public bool IsValid(IWowUnit unit)
        {
            return !Blacklist.Any(e => e == unit.DisplayId);
        }
    }
}