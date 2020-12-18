﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.Data.CombatLog.Objects
{
    public class BasicCombatLogEntrySubject : ISubjectBasicCombatLogEntry
    {
        private BasicCombatLogEntry _Entry = null;
        private HashSet<IObserverBasicCombatLogEntry> _Observers = new();

        public void Next(BasicCombatLogEntry entry)
        {
            _Entry = entry;
            Notify();
        }
        
        public void Register(IObserverBasicCombatLogEntry observer)
        {
            _Observers.Add(observer);
        }

        public void Unregister(IObserverBasicCombatLogEntry observer)
        {
            _Observers.Remove(observer);
        }

        public void Notify()
        {
            _Observers.ToList().ForEach(o => o.CombatLogChanged(_Entry));
        }
    }
}
