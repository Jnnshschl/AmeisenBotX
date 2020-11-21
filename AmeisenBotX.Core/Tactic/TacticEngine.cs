using AmeisenBotX.Core.Statemachine.Enums;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Tactic
{
    public class TacticEngine
    {
        public TacticEngine()
        {
            Tactics = new SortedList<int, ITactic>();
        }

        private SortedList<int, ITactic> Tactics { get; set; }

        public bool Execute(CombatClassRole role, bool isMelee, out bool handlesMovement, out bool allowAttacking)
        {
            if (Tactics.Count > 0)
            {
                foreach (ITactic tactic in Tactics.Values)
                {
                    if (tactic.ExecuteTactic(role, isMelee, out handlesMovement, out allowAttacking)) return true;
                }
            }

            handlesMovement = false;
            allowAttacking = true;
            return false;
        }

        public void LoadTactics(SortedList<int, ITactic> tactics)
        {
            Tactics = tactics;
        }

        public void Reset()
        {
            Tactics.Clear();
        }
    }
}