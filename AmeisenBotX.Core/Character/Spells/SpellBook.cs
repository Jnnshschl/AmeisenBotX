using AmeisenBotX.Core.Character.Spells.Objects;
using AmeisenBotX.Core.Hook;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Character.Spells
{
    public class SpellBook
    {
        public SpellBook(HookManager hookManager)
        {
            HookManager = hookManager;
            Update();
        }

        public List<Spell> Spells { get; private set; }

        private HookManager HookManager { get; }

        public void Update()
        {
            try
            {
                string rawSpells = HookManager.GetSpells();
                Spells = JsonConvert.DeserializeObject<List<Spell>>(rawSpells);
            }
            catch
            {
                Spells = new List<Spell>();
            }
        }

        public bool IsSpellKnown(string spellname)
            => Spells.Any(e => string.Equals(e.Name, spellname, StringComparison.OrdinalIgnoreCase));
    }
}
