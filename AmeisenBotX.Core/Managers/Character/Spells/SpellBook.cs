﻿using AmeisenBotX.Core.Managers.Character.Spells.Objects;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using AmeisenBotX.Wow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using AmeisenBotX.Core.Managers.Character;

namespace AmeisenBotX.Core.Managers.Character.Spells
{
    public class SpellBook : ISpellBook
    {
        public SpellBook(IWowInterface wowInterface)
        {
            Wow = wowInterface;
        }

        public delegate void SpellBookUpdate();

        public event SpellBookUpdate OnSpellBookUpdate;

        public IEnumerable<Spell> Spells { get; private set; }

        private IWowInterface Wow { get; }

        public Spell GetSpellByName(string spellname)
        {
            return Spells?.FirstOrDefault(e => string.Equals(e.Name, spellname, StringComparison.OrdinalIgnoreCase));
        }

        public bool IsSpellKnown(string spellname)
        {
            return Spells != null && Spells.Any(e => string.Equals(e.Name, spellname, StringComparison.OrdinalIgnoreCase));
        }

        public bool TryGetSpellByName(string spellname, out Spell spell)
        {
            spell = GetSpellByName(spellname);
            return spell != null;
        }

        public void Update()
        {
            string rawSpells = Wow.GetSpells();

            try
            {
                Spells = JsonSerializer.Deserialize<List<Spell>>(rawSpells, new JsonSerializerOptions() { AllowTrailingCommas = true, NumberHandling = JsonNumberHandling.AllowReadingFromString })
                    .OrderBy(e => e.Name)
                    .ThenByDescending(e => e.Rank);

                OnSpellBookUpdate?.Invoke();
            }
            catch (Exception e)
            {
                AmeisenLogger.I.Log("CharacterManager", $"Failed to parse Spells JSON:\n{rawSpells}\n{e}", LogLevel.Error);
            }
        }
    }
}