using AmeisenBotX.Core.Data.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Statemachine.Utils
{
    public class AuraManager
    {
        public AuraManager(TimeSpan minUpdateTime, GetAurasFunction getAurasFunction)
        {
            BuffsToKeepActive = new Dictionary<string, CastFunction>();
            DebuffsToKeepActive = new Dictionary<string, CastFunction>();
            MinUpdateTime = minUpdateTime;
            GetAuras = getAurasFunction;
        }

        public delegate bool CastFunction();

        public delegate bool DispellBuffsFunction();

        public delegate bool DispellDebuffsFunction();

        public delegate List<WowAura> GetAurasFunction();

        public List<WowAura> Auras { get; private set; }

        public List<WowAura> Buffs => Auras.Where(e => !e.IsHarmful).ToList();

        public Dictionary<string, CastFunction> BuffsToKeepActive { get; set; }

        public List<WowAura> Debuffs => Auras.Where(e => e.IsHarmful).ToList();

        public Dictionary<string, CastFunction> DebuffsToKeepActive { get; set; }

        public DispellBuffsFunction DispellBuffs { get; set; }

        public DispellDebuffsFunction DispellDebuffs { get; set; }

        public GetAurasFunction GetAuras { get; set; }

        public DateTime LastBuffUpdate { get; private set; }

        public TimeSpan MinUpdateTime { get; private set; }

        public bool Tick(bool forceUpdate = false)
        {
            if (DateTime.Now - LastBuffUpdate > MinUpdateTime
                || forceUpdate)
            {
                Auras = GetAuras();
                LastBuffUpdate = DateTime.Now;
            }

            if (Auras == null || Auras.Count == 0)
            {
                return false;
            }

            if (BuffsToKeepActive?.Count > 0 && Buffs != null)
            {
                foreach (KeyValuePair<string, CastFunction> keyValuePair in BuffsToKeepActive)
                {
                    if (!Buffs.Any(e => e.Name.Equals(keyValuePair.Key, StringComparison.OrdinalIgnoreCase))
                        && keyValuePair.Value())
                    {
                        return true;
                    }
                }
            }

            if (Buffs?.Count > 0 && DispellBuffs != null)
            {
                DispellBuffs();
            }

            if (DebuffsToKeepActive?.Count > 0 && Debuffs != null)
            {
                foreach (KeyValuePair<string, CastFunction> keyValuePair in DebuffsToKeepActive)
                {
                    if (!Debuffs.Any(e => e.Name.Equals(keyValuePair.Key, StringComparison.OrdinalIgnoreCase))
                        && keyValuePair.Value())
                    {
                        return true;
                    }
                }
            }

            if (Debuffs?.Count > 0 && DispellDebuffs != null)
            {
                DispellDebuffs();
            }

            return false;
        }
    }
}