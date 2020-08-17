using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Statemachine.Utils
{
    public class GroupAuraManager
    {
        public GroupAuraManager(WowInterface wowInterface)
        {
            WowInterface = wowInterface;
            SpellsToKeepActiveOnParty = new List<(string, CastSpellOnUnit)>();
            RemoveBadAurasSpells = new List<((string, DispelTypes), CastSpellOnUnit)>();
            LastBuffed = new Dictionary<ulong, TimegatedEvent>();
        }

        public delegate bool CastSpellOnUnit(string spellName, ulong guid);

        public List<((string, DispelTypes), CastSpellOnUnit)> RemoveBadAurasSpells { get; private set; }

        public List<(string, CastSpellOnUnit)> SpellsToKeepActiveOnParty { get; private set; }

        private Dictionary<ulong, TimegatedEvent> LastBuffed { get; }

        private WowInterface WowInterface { get; }

        public bool Tick()
        {
            if (SpellsToKeepActiveOnParty?.Count > 0)
            {
                foreach (WowUnit wowUnit in WowInterface.ObjectManager.Partymembers.Where(e => e.Guid != WowInterface.ObjectManager.PlayerGuid && !e.IsDead))
                {
                    foreach ((string, CastSpellOnUnit) auraCombo in SpellsToKeepActiveOnParty)
                    {
                        if (!wowUnit.HasBuffByName(auraCombo.Item1))
                        {
                            if (!LastBuffed.ContainsKey(wowUnit.Guid))
                            {
                                LastBuffed.Add(wowUnit.Guid, new TimegatedEvent(TimeSpan.FromSeconds(30)));
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