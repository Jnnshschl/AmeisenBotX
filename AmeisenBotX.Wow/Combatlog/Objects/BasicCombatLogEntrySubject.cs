using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Wow.Combatlog.Objects
{
    public class BasicCombatlogEntrySubject : ISubjectBasicCombatlogEntry
    {
        private readonly HashSet<IObserverBasicCombatlogEntry> _Observers = new();
        private BasicCombatlogEntry _Entry = null;

        public void Next(BasicCombatlogEntry entry)
        {
            _Entry = entry;
            Notify();
        }

        public void Notify()
        {
            _Observers.ToList().ForEach(o => o.CombatLogChanged(_Entry));
        }

        public void Register(IObserverBasicCombatlogEntry observer)
        {
            _Observers.Add(observer);
        }

        public void Unregister(IObserverBasicCombatlogEntry observer)
        {
            _Observers.Remove(observer);
        }
    }
}