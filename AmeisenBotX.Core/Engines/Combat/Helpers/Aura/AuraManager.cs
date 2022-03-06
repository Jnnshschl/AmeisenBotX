using AmeisenBotX.Core.Engines.Combat.Helpers.Aura.Objects;
using AmeisenBotX.Wow.Objects.Raw;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Combat.Helpers.Aura
{
    public class AuraManager
    {
        public AuraManager(AmeisenBotInterfaces bot)
        {
            Bot = bot;
            Jobs = new();
        }

        public AmeisenBotInterfaces Bot { get; }

        public List<IAuraJob> Jobs { get; set; }

        public bool Tick(IEnumerable<RawWowAura> auras)
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