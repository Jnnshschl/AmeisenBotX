using AmeisenBotX.Core.Character.Spells.Objects;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using AmeisenBotX.Wow;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Character.Spells
{
    public class SpellBook
    {
        public SpellBook(IWowInterface wowInterface)
        {
            Wow = wowInterface;

            Spells = new();

            JsonSerializerSettings = new()
            {
                Error = (sender, errorArgs) => errorArgs.ErrorContext.Handled = true
            };
        }

        public delegate void SpellBookUpdate();

        public event SpellBookUpdate OnSpellBookUpdate;

        public JsonSerializerSettings JsonSerializerSettings { get; }

        public List<Spell> Spells { get; private set; }

        private IWowInterface Wow { get; }

        public Spell GetSpellByName(string spellname)
        {
            return Spells?.FirstOrDefault(e => string.Equals(e.Name, spellname, StringComparison.OrdinalIgnoreCase));
        }

        public bool IsSpellKnown(string spellname)
        {
            return Spells != null && Spells.Any(e => string.Equals(e.Name, spellname, StringComparison.OrdinalIgnoreCase));
        }

        public void Update()
        {
            string rawSpells = Wow.LuaGetSpells();

            try
            {
                Spells = JsonConvert.DeserializeObject<List<Spell>>(rawSpells, JsonSerializerSettings)
                    .OrderBy(e => e.Name)
                    .ThenByDescending(e => e.Rank)
                    .ToList();

                OnSpellBookUpdate?.Invoke();
            }
            catch (Exception e)
            {
                AmeisenLogger.I.Log("CharacterManager", $"Failed to parse Spells JSON:\n{rawSpells}\n{e}", LogLevel.Error);
            }
        }
    }
}