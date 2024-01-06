using AmeisenBotX.Core.Engines.Combat.Helpers.Targets.Priority.Basic;
using AmeisenBotX.Wow.Objects;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Combat.Helpers.Targets.Priority
{
    public class TargetPriorityManager : ITargetPrioritizer
    {
        public TargetPriorityManager()
        {
            Prioritizers = [];
            ListTargetPrioritizer = new();
        }

        public TargetPriorityManager(ITargetPrioritizer validator)
        {
            Prioritizers = [validator];
            ListTargetPrioritizer = new();
        }

        public TargetPriorityManager(IEnumerable<ITargetPrioritizer> validators)
        {
            Prioritizers = new(validators);
            ListTargetPrioritizer = new();
        }

        public ListTargetPrioritizer ListTargetPrioritizer { get; }

        public List<ITargetPrioritizer> Prioritizers { get; }

        public void Add(ITargetPrioritizer prioritizer)
        {
            Prioritizers.Add(prioritizer);
        }

        public bool HasPriority(IWowUnit unit)
        {
            return Prioritizers.Any(e => e.HasPriority(unit));
        }
    }
}