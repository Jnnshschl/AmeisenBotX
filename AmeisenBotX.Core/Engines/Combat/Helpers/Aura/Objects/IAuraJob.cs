using AmeisenBotX.Wow.Objects;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Combat.Helpers.Aura.Objects
{
    public interface IAuraJob
    {
        bool Run(IEnumerable<IWowAura> auras);
    }
}