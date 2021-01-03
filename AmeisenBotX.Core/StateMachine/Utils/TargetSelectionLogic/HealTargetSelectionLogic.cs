using AmeisenBotX.Core.Data.Objects.WowObjects;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Statemachine.Utils.TargetSelectionLogic
{
    public class HealTargetSelectionLogic : BasicTargetSelectionLogic
    {
        public override bool SelectTarget(out IEnumerable<WowUnit> possibleTargets)
        {
            List<WowUnit> healableUnits = new List<WowUnit>(WowInterface.I.ObjectManager.Partymembers)
            {
                WowInterface.I.ObjectManager.Player
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