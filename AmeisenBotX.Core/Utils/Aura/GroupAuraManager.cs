using AmeisenBotX.Common.Utils;
using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Core.Data.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Utils.Aura
{
    public class GroupAuraManager
    {
        public GroupAuraManager(WowInterface wowInterface)
        {
            WowInterface = wowInterface;
            SpellsToKeepActiveOnParty = new();
            RemoveBadAurasSpells = new();
            LastBuffed = new();
        }

        public delegate bool CastSpellOnUnit(string spellName, ulong guid);

        public List<((string, WowDispelTypes), CastSpellOnUnit)> RemoveBadAurasSpells { get; private set; }

        public List<(string, CastSpellOnUnit)> SpellsToKeepActiveOnParty { get; private set; }

        private Dictionary<ulong, TimegatedEvent> LastBuffed { get; }

        private WowInterface WowInterface { get; }

        public bool Tick()
        {
            if (SpellsToKeepActiveOnParty?.Count > 0)
            {
                foreach (WowUnit wowUnit in WowInterface.Objects.Partymembers.Where(e => e.Guid != WowInterface.Player.Guid && !e.IsDead))
                {
                    foreach ((string, CastSpellOnUnit) auraCombo in SpellsToKeepActiveOnParty)
                    {
                        if (!wowUnit.HasBuffByName(auraCombo.Item1))
                        {
                            if (!LastBuffed.ContainsKey(wowUnit.Guid))
                            {
                                LastBuffed.Add(wowUnit.Guid, new(TimeSpan.FromSeconds(30)));
                            }
                            else if (LastBuffed[wowUnit.Guid].Run())
                            {
                                return auraCombo.Item2.Invoke(auraCombo.Item1, wowUnit.Guid);
                            }
                        }
                    }
                }
            }

            // TODO: recognize bad spells and dispell them
            // if (RemoveBadAurasSpells?.Count > 0)
            // {
            //     foreach (WowUnit wowUnit in WowInterface.ObjectManager.Partymembers)
            //     {
            //         foreach (WowAura wowAura in wowUnit.Auras.Where(e => e.IsHarmful))
            //         {
            //             foreach (((string, DispelType), CastSpellOnUnit) dispelCombo in RemoveBadAurasSpells)
            //             {
            //
            //             }
            //         }
            //     }
            // }

            return false;
        }
    }
}