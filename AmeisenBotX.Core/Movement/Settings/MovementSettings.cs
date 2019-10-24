using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.Movement.Settings
{
    public class MovementSettings
    {
        public double WaypointDoneThreshold { get; set; } = 4.0;

        public float MaxVelocity { get; set; } = 6.0f;
    }
}
