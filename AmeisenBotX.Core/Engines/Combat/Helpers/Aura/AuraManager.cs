using AmeisenBotX.Core.Engines.Combat.Helpers.Aura.Objects;
using AmeisenBotX.Wow.Objects;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Combat.Helpers.Aura
{
    public class AuraManager(AmeisenBotInterfaces bot)
    {
        public AmeisenBotInterfaces Bot { get; } = bot;

        public List<IAuraJob> Jobs { get; set; } = [];

        public bool Tick(IEnumerable<IWowAura> auras)
        {
            foreach (IAuraJob job in Jobs)
            {
                if (job.Run(auras))
                {
                    return true;
                }
            }

            return false;
        }
    }
}