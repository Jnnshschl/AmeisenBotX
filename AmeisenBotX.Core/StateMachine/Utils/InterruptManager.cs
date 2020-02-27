using AmeisenBotX.Core.Data.Objects.WowObject;
using System.Collections.Generic;

namespace AmeisenBotX.Core.StateMachine.Utils
{
    public class InterruptManager
    {
        public InterruptManager(WowUnit unitToWatch, SortedList<int, CastInterruptFunction> interruptSpells)
        {
            UnitToWatch = unitToWatch;
            InterruptSpells = interruptSpells;
        }

        public delegate bool CastInterruptFunction();

        public SortedList<int, CastInterruptFunction> InterruptSpells { get; set; }

        private WowUnit UnitToWatch { get; set; }

        public bool Tick()
        {
            if (InterruptSpells != null && InterruptSpells.Count > 0 && UnitToWatch != null && UnitToWatch.IsCasting)
            {
                foreach (KeyValuePair<int, CastInterruptFunction> keyValuePair in InterruptSpells)
                {
                    if (keyValuePair.Value.Invoke())
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}