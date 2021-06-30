using AmeisenBotX.Wow.Objects.Enums;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Tactic
{
    public class TacticEngine
    {
        public TacticEngine()
        {
            Tactics = new();
        }

        private SortedList<int, ITactic> Tactics { get; set; }

        public bool Execute(WowRole role, bool isMelee, out bool preventMovement, out bool allowAttacking)
        {
            if (Tactics.Count > 0)
            {
                foreach (ITactic tactic in Tactics.Values)
                {
                    if (tactic.ExecuteTactic(role, isMelee, out preventMovement, out allowAttacking))
                    {
                        return true;
                    }
                }
            }

            preventMovement = false;
            allowAttacking = true;
            return false;
        }

        public bool HasTactics()
        {
            return Tactics.Count > 0;
        }

        public void LoadTactics(params ITactic[] tactics)
        {
            Tactics = new();

            for (int i = 0; i < tactics.Length; ++i)
            {
                Tactics.Add(i, tactics[i]);
            }
        }

        public void Reset()
        {
            Tactics.Clear();
        }
    }
}