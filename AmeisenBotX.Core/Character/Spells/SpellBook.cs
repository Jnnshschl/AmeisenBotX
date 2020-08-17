using AmeisenBotX.Core.Character.Spells.Objects;
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
        public SpellBook(WowInterface wowInterface)
        {
            WowInterface = wowInterface;

            Spells = new List<Spell>();

            JsonSerializerSettings = new JsonSerializerSettings()
            {
                Error = (sender, errorArgs) => errorArgs.ErrorContext.Handled = true
            };
        }

        public delegate void SpellBookUpdate();

        public event SpellBookUpdate OnSpellBookUpdate;

        public JsonSerializerSettings JsonSerializerSettings { get; }

        public List<Spell> Spells { get; private set; }

        private WowInterface WowInterface { get; }

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
            string rawSpells = WowInterface.HookManager.GetSpells();

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