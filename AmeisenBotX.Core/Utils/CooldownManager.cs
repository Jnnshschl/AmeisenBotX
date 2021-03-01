using AmeisenBotX.Core.Character.Spells.Objects;
using System;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Utils
{
    public class CooldownManager
    {
        public CooldownManager(List<Spell> spells)
        {
            Cooldowns = new();

            if (spells != null)
            {
                foreach (Spell spell in spells)
                {
                    if (!Cooldowns.ContainsKey(spell.Name))
                    {
                        Cooldowns.Add(spell.Name, DateTime.Now);
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

            if (Cooldowns.ContainsKey(spellname))
            {
                return (int)(Cooldowns[spellname] - DateTime.Now).TotalMilliseconds;
            }

            return 0;
        }

        public bool IsSpellOnCooldown(string spellname)
        {
            if (string.IsNullOrWhiteSpace(spellname))
            {
                return false;
            }

            if (Cooldowns.TryGetValue(spellname.ToUpperInvariant(), out DateTime dateTime))
            {
                return dateTime > DateTime.Now;
            }

            return false;
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
                Cooldowns.Add(spellname, DateTime.Now + TimeSpan.FromMilliseconds(cooldownLeftMs));
            }
            else
            {
                Cooldowns[spellname] = DateTime.Now + TimeSpan.FromMilliseconds(cooldownLeftMs);
            }

            return true;
        }
    }
}