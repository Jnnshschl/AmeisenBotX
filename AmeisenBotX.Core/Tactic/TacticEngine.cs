using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Statemachine.Enums;
using AmeisenBotX.Core.Tactic.Bosses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AmeisenBotX.Core.Tactic
{
    public class TacticEngine
    {
        public TacticEngine()
        {
            Tactics = new SortedList<int, ITactic>();
        }

        private SortedList<int, ITactic> Tactics { get; set; }

        public void LoadTactics(SortedList<int, ITactic> tactics)
        {
            Tactics = tactics;
        }

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

        public void Reset()
        {
            Tactics.Clear();
        }
    }
}
