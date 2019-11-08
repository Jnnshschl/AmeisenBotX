using AmeisenBotX.Core.Character.Spells.Objects;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
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

        public bool IsSpellKnown(string spellname)
            => Spells.Any(e => string.Equals(e.Name, spellname, StringComparison.OrdinalIgnoreCase));

        public void Update()
        {
            string rawSpells = HookManager.GetSpells();

            try
            {
                Spells = JsonConvert.DeserializeObject<List<Spell>>(rawSpells);
            }
            catch (Exception e)
            {
                AmeisenLogger.Instance.Log($"Failed to parse Spells JSON:\n{rawSpells}\n{e.ToString()}", LogLevel.Error);
                Spells = new List<Spell>();
            }
        }
    }
}
