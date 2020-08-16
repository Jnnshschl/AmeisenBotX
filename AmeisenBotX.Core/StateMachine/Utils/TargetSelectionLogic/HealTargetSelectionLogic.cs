using AmeisenBotX.Core.Data.Objects.WowObject;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Statemachine.Utils.TargetSelectionLogic
{
    public class HealTargetSelectionLogic : ITargetSelectionLogic
    {
        public HealTargetSelectionLogic(WowInterface wowInterface)
        {
            WowInterface = wowInterface;
        }

        public IEnumerable<string> PriorityTargets { get; set; }

        private WowInterface WowInterface { get; }

        public void Reset()
        {
        }

        public bool SelectTarget(out IEnumerable<WowUnit> possibleTargets)
        {
            IEnumerable<WowUnit> healableUnits = WowInterface.ObjectManager.Partymembers;
            // healableUnits = healableUnits.Concat(WowInterface.ObjectManager.PartyPets);

            // order by type id, so that players have priority
            possibleTargets = healableUnits
                .Where(e => e.Health < e.MaxHealth && !e.IsDead)
                .OrderByDescending(e => e.Type)
                .ThenByDescending(e => e.MaxHealth - e.Health);

            return possibleTargets.Any();
        }
    }
}