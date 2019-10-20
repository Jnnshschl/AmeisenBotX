using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.Movement.Settings
{
    public class MovementSettings
    {
        public double WaypointCheckThreshold { get; set; } = 5.0;

        public int MaxTries { get; internal set; } = 32;
    }
}
