using AmeisenBotX.Core.Managers.Character.Spells.Objects;
using System;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Combat.Helpers
{
    public class CooldownManager
    {
        public CooldownManager(IEnumerable<Spell> spells)
        {
            Cooldowns = [];

            if (spells != null)
            {
                foreach (Spell spell in spells)
                {
                    if (!Cooldowns.ContainsKey(spell.Name))
                    {
                        Cooldowns.Add(spell.Name, DateTime.UtcNow);
                    }
                }
            }
        }

        public Dictionary<string, DateTime> Cooldowns { get; }

        public int GetSpellCooldown(string spellname)
        {
            if (string.IsNullOrWhiteSpace(spellname))
            {
                return 0;
            }

            spellname = spellname.ToUpperInvariant();

            return Cooldowns.ContainsKey(spellname) ? (int)(Cooldowns[spellname] - DateTime.UtcNow).TotalMilliseconds : 0;
        }

        public bool IsSpellOnCooldown(string spellname)
        {
            return !string.IsNullOrWhiteSpace(spellname)
&& Cooldowns.TryGetValue(spellname.ToUpperInvariant(), out DateTime dateTime) && dateTime > DateTime.UtcNow;
        }

        public bool SetSpellCooldown(string spellname, int cooldownLeftMs)
        {
            if (string.IsNullOrWhiteSpace(spellname))
            {
                return false;
            }

            spellname = spellname.ToUpperInvariant();

            if (!Cooldowns.ContainsKey(spellname))
            {
                Cooldowns.Add(spellname, DateTime.UtcNow + TimeSpan.FromMilliseconds(cooldownLeftMs));
            }
            else
            {
                Cooldowns[spellname] = DateTime.UtcNow + TimeSpan.FromMilliseconds(cooldownLeftMs);
            }

            return true;
        }
    }
}