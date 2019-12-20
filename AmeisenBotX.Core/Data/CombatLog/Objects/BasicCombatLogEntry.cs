using AmeisenBotX.Core.Data.CombatLog.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.Data.CombatLog.Objects
{
    [Serializable]
    public class BasicCombatLogEntry
    {
        public BasicCombatLogEntry(CombatLogEntryType combatLogEntryType, CombatLogEntrySubtype combatLogEntrySubtype)
        {
            CombatLogEntryType = combatLogEntryType;
            CombatLogEntrySubtype = combatLogEntrySubtype;
        }

        public CombatLogEntryType CombatLogEntryType { get; }

        public CombatLogEntrySubtype CombatLogEntrySubtype { get; }
    }
}
