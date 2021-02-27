using AmeisenBotX.Core.Data.Objects.Raw;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Fsm.Utils.Auras.Objects
{
    public class KeepActiveAuraJob : IAuraJob
    {
        public KeepActiveAuraJob(string name, Func<bool> action)
        {
            Name = name;
            Action = action;
        }

        public Func<bool> Action { get; set; }

        public string Name { get; set; }

        public bool Run(IEnumerable<WowAura> auras)
            => !auras.Any(e => e.Name.Equals(Name, StringComparison.OrdinalIgnoreCase)) && Action();
    }
}