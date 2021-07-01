using System.Collections.Generic;

namespace AmeisenBotX.Wow.Combatlog
{
    public interface ICombatLogParser
    {
        void Parse(long timestamp, List<string> args);
    }
}