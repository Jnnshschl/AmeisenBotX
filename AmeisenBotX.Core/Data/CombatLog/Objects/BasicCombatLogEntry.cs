using AmeisenBotX.Core.Data.CombatLog.Enums;
using System;

namespace AmeisenBotX.Core.Data.CombatLog.Objects
{
    public class BasicCombatLogEntry
    {
        public BasicCombatLogEntry(CombatLogEntryType combatLogEntryType, CombatLogEntrySubtype combatLogEntrySubtype)
        {
            CombatLogEntryType = combatLogEntryType;
            CombatLogEntrySubtype = combatLogEntrySubtype;
        }

        public CombatLogEntrySubtype CombatLogEntrySubtype { get; }

        public CombatLogEntryType CombatLogEntryType { get; }
    }
}