using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Wow.Combatlog.Objects
{
    public class BasicCombatLogEntrySubject : ISubjectBasicCombatLogEntry
    {
        private readonly HashSet<IObserverBasicCombatLogEntry> _Observers = new();
        private BasicCombatLogEntry _Entry = null;

        public void Next(BasicCombatLogEntry entry)
        {
            _Entry = entry;
            Notify();
        }

        public void Notify()
        {
            _Observers.ToList().ForEach(o => o.CombatLogChanged(_Entry));
        }

        public void Register(IObserverBasicCombatLogEntry observer)
        {
            _Observers.Add(observer);
        }

        public void Unregister(IObserverBasicCombatLogEntry observer)
        {
            _Observers.Remove(observer);
        }
    }
}