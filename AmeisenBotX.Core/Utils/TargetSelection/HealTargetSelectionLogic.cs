using AmeisenBotX.Core.Data.Objects;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Utils.TargetSelection
{
    public class HealTargetSelectionLogic : BasicTargetSelectionLogic
    {
        public HealTargetSelectionLogic(WowInterface wowInterface) : base(wowInterface)
        {
        }

        public override bool SelectTarget(out IEnumerable<WowUnit> possibleTargets)
        {
            List<WowUnit> healableUnits = new(WowInterface.Objects.Partymembers)
            {
                WowInterface.Player
            };

            // healableUnits.AddRange(WowInterface.ObjectManager.PartyPets);

            possibleTargets = healableUnits
                .Where(e => !e.IsDead && e.Health < e.MaxHealth)
                .OrderByDescending(e => e.Type)
                .ThenByDescending(e => e.MaxHealth - e.Health);

            return possibleTargets.Any();
        }
    }
}