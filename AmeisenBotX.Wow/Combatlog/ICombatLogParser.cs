using System;
using System.Collections.Generic;

namespace AmeisenBotX.Wow.Combatlog
{
    public interface ICombatLogParser
    {
        event Action<ulong, ulong, int> OnDamage;

        event Action<ulong, ulong> OnPartyKill;

        event Action<ulong> OnUnitDied;

        void Parse(long timestamp, List<string> args);
    }
}