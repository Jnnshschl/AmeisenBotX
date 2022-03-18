using AmeisenBotX.Wow.Objects;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Combat.Helpers.Targets.Priority.Basic
{
    public class ListTargetPrioritizer : ITargetPrioritizer
    {
        public ListTargetPrioritizer(IEnumerable<int> priorityDisplayIds = null)
        {
            PriorityDisplayIds = priorityDisplayIds ?? new List<int>();
        }

        public IEnumerable<int> PriorityDisplayIds { get; set; }

        public bool HasPriority(IWowUnit unit)
        {
            return PriorityDisplayIds.Any(e => e == unit.DisplayId);
        }
    }
}