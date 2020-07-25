using AmeisenBotX.Core.Grinding.Objects;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace AmeisenBotX.Core.Grinding.Profiles.Profiles.Alliance.Group
{
    public class UltimateGrinding1To80 : IGrindingProfile
    {
        public bool RandomizeSpots { get; } = true;

        public List<GrindingSpot> Spots { get; } = new List<GrindingSpot>()
        {
            new GrindingSpot(new Vector3(-9039, -265, 74), 32.0, 1, 3),
            new GrindingSpot(new Vector3(-8890, -266, 79), 36.0, 1, 3),
            new GrindingSpot(new Vector3(-8952, -393, 70), 42.0, 3, 5),
            new GrindingSpot(new Vector3(-9069, -371, 73), 50.0, 3, 5),
            new GrindingSpot(new Vector3(-9155, 70, 77), 76.0, 5, 10),
            new GrindingSpot(new Vector3(-9453, 470, 53), 64.0, 10, 12),
            new GrindingSpot(new Vector3(-10014, 653, 37), 76.0, 10, 12),
            new GrindingSpot(new Vector3(-10238, 967, 38), 56.0, 11, 13),
            new GrindingSpot(new Vector3(-10498, 1326, 43), 48.0, 12, 14),
            new GrindingSpot(new Vector3(-11023, 1494, 43), 48.0, 13, 15),
        };
    }
}
