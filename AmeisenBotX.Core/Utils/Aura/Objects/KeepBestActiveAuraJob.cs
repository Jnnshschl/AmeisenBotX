using AmeisenBotX.Core.Data.Objects.Raw;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Fsm.Utils.Auras.Objects
{
    public class KeepBestActiveAuraJob : IAuraJob
    {
        public KeepBestActiveAuraJob(IEnumerable<(string, Func<bool>)> actions)
        {
            Actions = actions;
        }

        public IEnumerable<(string, Func<bool>)> Actions { get; set; }

        public bool Run(IEnumerable<WowAura> auras)
        {
            foreach ((string name, Func<bool> actionFunc) in Actions)
            {
                if (auras.Any(e => e.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                {
                    return false;
                }
                else if (actionFunc())
                {
                    return true;
                }
            }

            return false;
        }
    }
}