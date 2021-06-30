using AmeisenBotX.Wow.Cache;
using AmeisenBotX.Wow.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Fsm.Utils.Auras.Objects
{
    public class KeepActiveAuraJob : IAuraJob
    {
        public KeepActiveAuraJob(IAmeisenBotDb db, string name, Func<bool> action)
        {
            Db = db;
            Name = name;
            Action = action;
        }

        public Func<bool> Action { get; set; }

        public string Name { get; set; }

        private IAmeisenBotDb Db { get; }

        public bool Run(IEnumerable<RawWowAura> auras)
        {
            return auras != null && !auras.Any(e => Db.GetSpellName(e.SpellId).Equals(Name, StringComparison.OrdinalIgnoreCase)) && Action();
        }
    }
}