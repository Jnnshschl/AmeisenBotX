using AmeisenBotX.Core.Character.Spells.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.StateMachine.CombatClasses.Utils
{
    public class CooldownManager
    {
        public CooldownManager(List<Spell> spells)
        {
            Cooldowns = new Dictionary<string, DateTime>();

            foreach (Spell spell in spells)
            {
                if (!Cooldowns.ContainsKey(spell.Name.ToUpper()))
                {
                    Cooldowns.Add(spell.Name.ToUpper(), DateTime.Now);
                }
            }
        }

        public Dictionary<string, DateTime> Cooldowns { get; }

        public bool SetSpellCooldown(string spellname, int cooldownLeftMs)
        {
            if (Cooldowns.ContainsKey(spellname.ToUpper()))
            {
                Cooldowns.Add(spellname.ToUpper(), DateTime.Now + TimeSpan.FromMilliseconds(cooldownLeftMs));
                return true;
            }

            return false;
        }

        public bool IsSpellOnCooldown(string spellname)
            => Cooldowns.TryGetValue(spellname, out DateTime dateTime)
            && dateTime > DateTime.Now;
    }
}
