using AmeisenBotX.Common.Utils;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Combat.Helpers.Aura
{
    public class GroupAuraManager(AmeisenBotInterfaces bot)
    {
        public delegate bool CastSpellOnUnit(string spellName, ulong guid);

        public List<((string, WowDispelType), CastSpellOnUnit)> RemoveBadAurasSpells { get; private set; } = [];

        public List<(string, CastSpellOnUnit)> SpellsToKeepActiveOnParty { get; private set; } = [];

        private AmeisenBotInterfaces Bot { get; } = bot;

        private Dictionary<ulong, TimegatedEvent> LastBuffed { get; } = [];

        public bool Tick()
        {
            if (SpellsToKeepActiveOnParty?.Count > 0)
            {
                foreach (IWowUnit wowUnit in Bot.Objects.Partymembers.Where(e => e.Guid != Bot.Wow.PlayerGuid && !e.IsDead))
                {
                    foreach ((string, CastSpellOnUnit) auraCombo in SpellsToKeepActiveOnParty)
                    {
                        if (!wowUnit.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == auraCombo.Item1))
                        {
                            if (!LastBuffed.TryGetValue(wowUnit.Guid, out TimegatedEvent value))
                            {
                                LastBuffed.Add(wowUnit.Guid, new(TimeSpan.FromSeconds(30)));
                            }
                            else if (value.Run())
                            {
                                return auraCombo.Item2.Invoke(auraCombo.Item1, wowUnit.Guid);
                            }
                        }
                    }
                }
            }

            // TODO: recognize bad spells and dispell them if (RemoveBadAurasSpells?.Count > 0) {
            // foreach (IWowUnit wowUnit in Bot.ObjectManager.Partymembers) { foreach (WowAura
            // wowAura in wowUnit.Auras.Where(e => e.IsHarmful)) { foreach (((string, DispelType),
            // CastSpellOnUnit) dispelCombo in RemoveBadAurasSpells) {
            //
            // } } } }

            return false;
        }
    }
}