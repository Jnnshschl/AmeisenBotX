using AmeisenBotX.Core.Data.Objects.Structs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace AmeisenBotX.Core.Statemachine.Utils
{
    public class AuraManager
    {
        public AuraManager(GetAurasFunction getAurasFunction)
        {
            BuffsToKeepActive = new Dictionary<string, CastFunction>();
            BuffsToKeepUpCondition = new Dictionary<string, Condition>();
            DebuffsToKeepActive = new Dictionary<string, CastFunction>();
            GetAuras = getAurasFunction;
        }

        public delegate bool CastFunction();

        public delegate bool DispellBuffsFunction();

        public delegate bool DispellDebuffsFunction();

        public delegate bool Condition();

        public delegate IEnumerable<WowAura> GetAurasFunction();

        public IEnumerable<WowAura> Auras { get; private set; }

        public IEnumerable<WowAura> Buffs => Auras;

        public Dictionary<string, CastFunction> BuffsToKeepActive { get; set; }
        
        public Dictionary<string, Condition> BuffsToKeepUpCondition { get; set; }

        public IEnumerable<WowAura> Debuffs => Auras;

        public Dictionary<string, CastFunction> DebuffsToKeepActive { get; set; }

        public DispellBuffsFunction DispellBuffs { get; set; }

        public DispellDebuffsFunction DispellDebuffs { get; set; }

        public GetAurasFunction GetAuras { get; set; }

        public bool Tick()
        {
            Auras = GetAuras();

            if (Auras == null || !Auras.Any())
            {
                return false;
            }

            if (Buffs != null)
            {
                if (BuffsToKeepActive?.Count > 0)
                {
                    foreach (KeyValuePair<string, CastFunction> keyValuePair in BuffsToKeepActive)
                    {
                        if (!Buffs.Any(e => e.SpellId != 0 && e.Name.Equals(keyValuePair.Key, StringComparison.OrdinalIgnoreCase))
                            && (!BuffsToKeepUpCondition.ContainsKey(keyValuePair.Key) || BuffsToKeepUpCondition[keyValuePair.Key]()) && keyValuePair.Value())
                        {
                            return true;
                        }
                    }
                }

                if (Buffs.Any() && DispellBuffs != null)
                {
                    DispellBuffs();
                }
            }

            if (Debuffs != null)
            {
                if (DebuffsToKeepActive?.Count > 0)
                {
                    foreach (KeyValuePair<string, CastFunction> keyValuePair in DebuffsToKeepActive)
                    {
                        if (!Debuffs.Any(e => e.SpellId != 0 && e.Name.Equals(keyValuePair.Key, StringComparison.OrdinalIgnoreCase))
                            && keyValuePair.Value())
                        {
                            return true;
                        }
                    }
                }

                if (Debuffs.Any() && DispellDebuffs != null)
                {
                    // DispellDebuffs();
                }
            }

            return false;
        }
    }
}