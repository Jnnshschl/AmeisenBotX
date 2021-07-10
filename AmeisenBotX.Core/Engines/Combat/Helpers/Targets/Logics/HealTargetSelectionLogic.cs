using AmeisenBotX.Core.Data.Objects;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Combat.Helpers.Targets.Logics
{
    public class HealTargetSelectionLogic : BasicTargetSelectionLogic
    {
        public HealTargetSelectionLogic(AmeisenBotInterfaces bot) : base(bot)
        {
        }

        public override bool SelectTarget(out IEnumerable<IWowUnit> possibleTargets)
        {
            List<IWowUnit> healableUnits = new(Bot.Objects.Partymembers)
            {
                Bot.Player
            };

            // healableUnits.AddRange(Bot.ObjectManager.PartyPets);

            possibleTargets = healableUnits
                .Where(e => !e.IsDead && e.Health < e.MaxHealth)
                .OrderByDescending(e => e.Type)
                .ThenByDescending(e => e.MaxHealth - e.Health);

            return possibleTargets.Any();
        }
    }
}