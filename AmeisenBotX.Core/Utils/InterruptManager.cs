using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Logging;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Utils
{
    public class InterruptManager
    {
        public InterruptManager()
        {
            InterruptSpells = new();
        }

        public delegate bool CastInterruptFunction(WowUnit target);

        public SortedList<int, CastInterruptFunction> InterruptSpells { get; set; }

        public bool Tick(IEnumerable<WowUnit> units)
        {
            if (InterruptSpells != null && InterruptSpells.Count > 0 && units != null && units.Any())
            {
                WowUnit selectedUnit = units.FirstOrDefault(e => e != null && e.IsCasting);

                if (selectedUnit != null)
                {
                    foreach (KeyValuePair<int, CastInterruptFunction> keyValuePair in InterruptSpells)
                    {
                        if (keyValuePair.Value(selectedUnit))
                        {
                            AmeisenLogger.I.Log("Interrupt", $"Interrupted \"{selectedUnit}\" using CastInterruptFunction: \"{keyValuePair.Key}\"");
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}