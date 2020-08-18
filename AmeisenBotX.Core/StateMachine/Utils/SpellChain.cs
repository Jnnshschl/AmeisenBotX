using System;

namespace AmeisenBotX.Core.Statemachine.Utils
{
    public static class SpellChain
    {
        public static bool Get(Func<string, bool> selectFunction, out string spellToCast, params string[] spells)
        {
            for (int i = 0; i < spells.Length; ++i)
            {
                if (selectFunction(spells[i]))
                {
                    spellToCast = spells[i];
                    return true;
                }
            }

            spellToCast = null;
            return false;
        }
    }
}