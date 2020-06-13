using AmeisenBotX.Core.Character.Spells.Objects;
using System;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Statemachine.Utils
{
    public class CooldownManager
    {
        public CooldownManager(List<Spell> spells)
        {
            Cooldowns = new Dictionary<string, DateTime>();

            if (spells != null)
            {
                foreach (Spell spell in spells)
                {
                    if (!Cooldowns.ContainsKey(spell.Name.ToUpper()))
                    {
                        Cooldowns.Add(spell.Name.ToUpper(), DateTime.Now);
                    }
                }
            }
        }

        public Dictionary<string, DateTime> Cooldowns { get; }

        public bool IsSpellOnCooldown(string spellname)
        {
            if (Cooldowns.ContainsKey(spellname.ToUpper()))
            {
                if (Cooldowns.TryGetValue(spellname.ToUpper(), out DateTime dateTime))
                {
                    return dateTime > DateTime.Now;
                }
            }

            return false;
        }

        public bool SetSpellCooldown(string spellname, int cooldownLeftMs)
        {
            if (!Cooldowns.ContainsKey(spellname.ToUpper()))
            {
                Cooldowns.Add(spellname.ToUpper(), DateTime.Now + TimeSpan.FromMilliseconds(cooldownLeftMs));
            }
            else
            {
                Cooldowns[spellname.ToUpper()] = DateTime.Now + TimeSpan.FromMilliseconds(cooldownLeftMs);
            }

            return true;
        }
    }
}