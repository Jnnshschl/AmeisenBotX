using AmeisenBotX.Wow.Objects.Raw;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Combat.Helpers.Aura.Objects
{
    public interface IAuraJob
    {
        bool Run(IEnumerable<RawWowAura> auras);
    }
}