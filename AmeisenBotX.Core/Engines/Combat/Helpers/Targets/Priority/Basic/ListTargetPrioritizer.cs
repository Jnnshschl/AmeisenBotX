using AmeisenBotX.Wow.Objects;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Combat.Helpers.Targets.Priority.Basic
{
    public class ListTargetPrioritizer(IEnumerable<int> priorityDisplayIds = null) : ITargetPrioritizer
    {
        public IEnumerable<int> PriorityDisplayIds { get; set; } = priorityDisplayIds ?? new List<int>();

        public bool HasPriority(IWowUnit unit)
        {
            return PriorityDisplayIds.Any(e => e == unit.DisplayId);
        }
    }
}