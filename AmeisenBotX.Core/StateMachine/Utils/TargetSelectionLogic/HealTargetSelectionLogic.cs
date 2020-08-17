using AmeisenBotX.Core.Data.Objects.WowObjects;
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
            List<WowUnit> healableUnits = new List<WowUnit>(WowInterface.ObjectManager.Partymembers);
            healableUnits.AddRange(WowInterface.ObjectManager.PartyPets);
            healableUnits.Add(WowInterface.ObjectManager.Player);

            // order by type id, so that players have priority
            possibleTargets = healableUnits
                .Where(e => e.Health < e.MaxHealth && !e.IsDead)
                .OrderByDescending(e => e.Type)
                .ThenByDescending(e => e.MaxHealth - e.Health);

            return possibleTargets.Any();
        }
    }
}