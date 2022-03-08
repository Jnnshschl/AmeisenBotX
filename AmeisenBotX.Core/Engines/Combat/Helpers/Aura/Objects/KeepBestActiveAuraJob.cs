using AmeisenBotX.Wow.Cache;
using AmeisenBotX.Wow.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Combat.Helpers.Aura.Objects
{
    public class KeepBestActiveAuraJob : IAuraJob
    {
        public KeepBestActiveAuraJob(IAmeisenBotDb db, IEnumerable<(string, Func<bool>)> actions)
        {
            Db = db;
            Actions = actions;
        }

        public IEnumerable<(string, Func<bool>)> Actions { get; set; }

        private IAmeisenBotDb Db { get; }

        public bool Run(IEnumerable<IWowAura> auras)
        {
            foreach ((string name, Func<bool> actionFunc) in Actions)
            {
                if (auras.Any(e => Db.GetSpellName(e.SpellId).Equals(name, StringComparison.OrdinalIgnoreCase)))
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