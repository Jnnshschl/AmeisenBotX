using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Data.Objects.Structs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Statemachine.Utils
{
    public class AuraManager
    {
        public AuraManager(GetAurasFunction getAurasFunction)
        {
            BuffsToKeepActive = new Dictionary<string, CastFunction>();
            DebuffsToKeepActive = new Dictionary<string, CastFunction>();
            GetAuras = getAurasFunction;
        }

        public delegate bool CastFunction();

        public delegate bool DispellBuffsFunction();

        public delegate bool DispellDebuffsFunction();

        public delegate WowAura[] GetAurasFunction();

        public WowAura[] Auras { get; private set; }

        public WowAura[] Buffs => Auras;

        public Dictionary<string, CastFunction> BuffsToKeepActive { get; set; }

        public WowAura[] Debuffs => Auras;

        public Dictionary<string, CastFunction> DebuffsToKeepActive { get; set; }

        public DispellBuffsFunction DispellBuffs { get; set; }

        public DispellDebuffsFunction DispellDebuffs { get; set; }

        public GetAurasFunction GetAuras { get; set; }

        public bool Tick()
        {
            Auras = GetAuras();

            if (Auras == null || Auras.Length == 0)
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
                            && keyValuePair.Value())
                        {
                            return true;
                        }
                    }
                }

                if (Buffs.Length > 0 && DispellBuffs != null)
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

                if (Debuffs.Length > 0 && DispellDebuffs != null)
                {
                    // DispellDebuffs();
                }
            }

            return false;
        }
    }
}