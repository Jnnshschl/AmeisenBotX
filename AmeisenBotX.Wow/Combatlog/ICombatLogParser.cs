using System;
using System.Collections.Generic;

namespace AmeisenBotX.Wow.Combatlog
{
    public interface ICombatlogParser
    {
        event Action<ulong, ulong, int, int, int> OnDamage;

        event Action<ulong, ulong, int, int, int> OnHeal;

        event Action<ulong, ulong> OnPartyKill;

        event Action<ulong> OnUnitDied;

        void Parse(long timestamp, List<string> args);
    }
}