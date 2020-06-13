using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Logging;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Statemachine.Utils
{
    public class InterruptManager
    {
        public InterruptManager(List<WowUnit> unitsToWatch, SortedList<int, CastInterruptFunction> interruptSpells)
        {
            UnitsToWatch = unitsToWatch;
            InterruptSpells = interruptSpells;
        }

        public delegate bool CastInterruptFunction(WowUnit target);

        public SortedList<int, CastInterruptFunction> InterruptSpells { get; set; }

        public List<WowUnit> UnitsToWatch { get; set; }

        public bool Tick()
        {
            if (InterruptSpells != null && InterruptSpells.Count > 0 && UnitsToWatch != null && UnitsToWatch.Count > 0)
            {
                WowUnit selectedUnit = UnitsToWatch.FirstOrDefault(e => e != null && e.IsCasting);

                if (selectedUnit != null)
                {
                    foreach (KeyValuePair<int, CastInterruptFunction> keyValuePair in InterruptSpells)
                    {
                        if (keyValuePair.Value(selectedUnit))
                        {
                            AmeisenLogger.Instance.Log("Interrupt", $"Interrupted \"{selectedUnit}\" using CastInterruptFunction: \"{keyValuePair.Key}\"");
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}