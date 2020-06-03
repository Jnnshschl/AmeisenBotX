using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
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
            RemoveBadAurasSpells = new List<((string, DispelType), CastSpellOnUnit)>();
        }

        public delegate bool CastSpellOnUnit(string spellName, ulong guid);

        public List<((string, DispelType), CastSpellOnUnit)> RemoveBadAurasSpells { get; private set; }

        public List<(string, CastSpellOnUnit)> SpellsToKeepActiveOnParty { get; private set; }

        private WowInterface WowInterface { get; }

        public bool Tick()
        {
            if (SpellsToKeepActiveOnParty?.Count > 0)
            {
                foreach (WowUnit wowUnit in WowInterface.ObjectManager.Partymembers.Where(e => e.Guid != WowInterface.ObjectManager.PlayerGuid))
                {
                    foreach ((string, CastSpellOnUnit) auraCombo in SpellsToKeepActiveOnParty)
                    {
                        if (!wowUnit.HasBuffByName(auraCombo.Item1))
                        {
                            return auraCombo.Item2.Invoke(auraCombo.Item1, wowUnit.Guid);
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