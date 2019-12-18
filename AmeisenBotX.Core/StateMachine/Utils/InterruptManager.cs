using AmeisenBotX.Core.Data.Objects.WowObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        private WowUnit UnitToWatch { get; set; }

        private SortedList<int, CastInterruptFunction> InterruptSpells { get; set; }

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
