using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.Data.CombatLog.Objects
{
    public interface ISubjectBasicCombatLogEntry
    {
        void Register(IObserverBasicCombatLogEntry observer);
        void Unregister(IObserverBasicCombatLogEntry observer);
        void Notify();
    }
}
