using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Statemachine.Utils
{
    public class AuraManager
    {
        public AuraManager(Dictionary<string, CastFunction> buffsToKeepActive, Dictionary<string, CastFunction> debuffsToKeepActive, TimeSpan minUpdateTime, GetBuffsFunction getBuffsFunction, GetDebuffsFunction getDebuffsFunction, DispellBuffsFunction dispellBuffsFunction, DispellDebuffsFunction dispellDebuffsFunction)
        {
            BuffsToKeepActive = buffsToKeepActive;
            DebuffsToKeepActive = debuffsToKeepActive;
            MinUpdateTime = minUpdateTime;
            DispellBuffs = dispellBuffsFunction;
            DispellDebuffs = dispellDebuffsFunction;
            GetBuffs = getBuffsFunction;
            GetDebuffs = getDebuffsFunction;
        }

        public AuraManager()
        {
            Buffs = new List<string>();
            Debuffs = new List<string>();
        }

        public delegate bool CastFunction();

        public delegate bool DispellBuffsFunction();

        public delegate bool DispellDebuffsFunction();

        public delegate List<string> GetBuffsFunction();

        public delegate List<string> GetDebuffsFunction();

        public List<string> Buffs { get; private set; }

        public Dictionary<string, CastFunction> BuffsToKeepActive { get; set; }

        public List<string> Debuffs { get; private set; }

        public Dictionary<string, CastFunction> DebuffsToKeepActive { get; set; }

        public DispellBuffsFunction DispellBuffs { get; set; }

        public DispellDebuffsFunction DispellDebuffs { get; set; }

        public GetBuffsFunction GetBuffs { get; set; }

        public GetDebuffsFunction GetDebuffs { get; set; }

        public DateTime LastBuffUpdate { get; private set; }

        public TimeSpan MinUpdateTime { get; private set; }

        public bool Tick(bool forceUpdate = false)
        {
            if (DateTime.Now - LastBuffUpdate > MinUpdateTime
                || forceUpdate)
            {
                Buffs = GetBuffs();
                Debuffs = GetDebuffs();
                LastBuffUpdate = DateTime.Now;
            }

            if (BuffsToKeepActive?.Count > 0 && Buffs != null)
            {
                foreach (KeyValuePair<string, CastFunction> keyValuePair in BuffsToKeepActive)
                {
                    if (!Buffs.Any(e => e.Contains(keyValuePair.Key.ToLower())))
                    {
                        if (keyValuePair.Value.Invoke())
                        {
                            return true;
                        }
                    }
                }
            }

            if (Buffs?.Count > 0 && DispellBuffs != null)
            {
                DispellBuffs.Invoke();
            }

            if (DebuffsToKeepActive?.Count > 0 && Debuffs != null)
            {
                foreach (KeyValuePair<string, CastFunction> keyValuePair in DebuffsToKeepActive)
                {
                    if (!Debuffs.Any(e => e.Contains(keyValuePair.Key.ToLower())))
                    {
                        if (keyValuePair.Value.Invoke())
                        {
                            return true;
                        }
                    }
                }
            }

            if (Debuffs?.Count > 0 && DispellDebuffs != null)
            {
                DispellDebuffs.Invoke();
            }

            return false;
        }
    }
}