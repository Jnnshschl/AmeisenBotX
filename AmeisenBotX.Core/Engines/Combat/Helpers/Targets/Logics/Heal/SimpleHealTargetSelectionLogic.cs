using AmeisenBotX.Wow.Objects;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Combat.Helpers.Targets.Logics.Heal
{
    public class SimpleHealTargetSelectionLogic : BasicTargetSelectionLogic
    {
        public SimpleHealTargetSelectionLogic(AmeisenBotInterfaces bot) : base(bot)
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
                .Where(e => TargetValidator.IsValid(e) && e.Health > 1 && e.Health < e.MaxHealth)
                .OrderByDescending(e => e.Type)
                .ThenByDescending(e => e.MaxHealth - e.Health);

            return possibleTargets.Any();
        }
    }
}