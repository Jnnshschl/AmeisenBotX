using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Pathfinding
{
    [Flags]
    public enum PathRequestFlags
    {
        None = 0,
        ChaikinCurve = 1 << 0
    }
}
