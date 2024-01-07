using AmeisenBotX.Wow.Cache;
using AmeisenBotX.Wow.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Combat.Helpers.Aura.Objects
{
    public class KeepActiveAuraJob(IAmeisenBotDb db, string name, Func<bool> action) : IAuraJob
    {
        public Func<bool> Action { get; set; } = action;

        public string Name { get; set; } = name;

        private IAmeisenBotDb Db { get; } = db;

        public bool Run(IEnumerable<IWowAura> auras)
        {
            return auras != null && !auras.Any(e => Db.GetSpellName(e.SpellId).Equals(Name, StringComparison.OrdinalIgnoreCase)) && Action();
        }
    }
}