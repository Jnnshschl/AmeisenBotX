using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.Data.CombatLog.Objects
{
    public interface IObserverBasicCombatLogEntry
    {
        void CombatLogChanged(BasicCombatLogEntry entry);
    }
}
