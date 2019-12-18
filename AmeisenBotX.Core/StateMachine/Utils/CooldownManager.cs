using AmeisenBotX.Core.Character.Spells.Objects;
using System;
using System.Collections.Generic;

namespace AmeisenBotX.Core.StateMachine.Utils
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

        public bool IsSpellOnCooldown(string spellname)
        {
            if (Cooldowns.ContainsKey(spellname.ToUpper()))
            {
                return Cooldowns.TryGetValue(spellname.ToUpper(), out DateTime dateTime)
                       && dateTime > DateTime.Now;
            }

            return false;
        }
    }
}
