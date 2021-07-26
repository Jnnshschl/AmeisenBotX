using AmeisenBotX.Core.Engines.Character.Spells.Objects;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using AmeisenBotX.Wow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AmeisenBotX.Core.Engines.Character.Spells
{
    public class SpellBook
    {
        public SpellBook(IWowInterface wowInterface)
        {
            Wow = wowInterface;

            Spells = new();
        }

        public delegate void SpellBookUpdate();

        public event SpellBookUpdate OnSpellBookUpdate;

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
            string rawSpells = Wow.GetSpells();

            try
            {
                Spells = JsonSerializer.Deserialize<List<Spell>>(rawSpells, new() { AllowTrailingCommas = true, NumberHandling = JsonNumberHandling.AllowReadingFromString })
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