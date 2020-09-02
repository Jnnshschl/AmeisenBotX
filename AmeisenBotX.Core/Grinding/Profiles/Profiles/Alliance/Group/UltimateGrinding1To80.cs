using AmeisenBotX.Core.Grinding.Objects;
using System.Collections.Generic;
using System.Numerics;

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
            new GrindingSpot(new Vector3(-9155, 70, 77), 76.0, 5, 8),
            new GrindingSpot(new Vector3(-9453, 470, 53), 64.0, 8, 11),
            new GrindingSpot(new Vector3(-10014, 653, 37), 76.0, 9, 12),
            new GrindingSpot(new Vector3(-10238, 967, 38), 56.0, 11, 13),
            new GrindingSpot(new Vector3(-10498, 1326, 43), 48.0, 12, 14),
            new GrindingSpot(new Vector3(-11023, 1494, 43), 100.0, 13, 16),
            new GrindingSpot(new Vector3(-11080, 944, 38), 100.0, 17, 18),
            new GrindingSpot(new Vector3(-11164, 721, 36), 64.0, 17, 18),
            new GrindingSpot(new Vector3(-11112, 609, 37), 64.0, 17, 18),
            new GrindingSpot(new Vector3(-10706, 635, 34), 64.0, 19, 20),
            new GrindingSpot(new Vector3(-10569, 545, 31), 64.0, 19, 20),
            new GrindingSpot(new Vector3(-10549, 415, 36), 128.0, 21, 26),
            new GrindingSpot(new Vector3(-10556, 231, 30), 128.0, 21, 26),
            new GrindingSpot(new Vector3(-11039, -228, 15), 76.0, 27, 29),
            new GrindingSpot(new Vector3(-11074, -513, 32), 56.0, 27, 29),
            new GrindingSpot(new Vector3(-10079, -3366, 20), 64.0, 30, 35),
            new GrindingSpot(new Vector3(-11071, -3455, 21), 56.0, 30, 35),
            new GrindingSpot(new Vector3(-10151, -3497, 23), 56.0, 30, 35),
            new GrindingSpot(new Vector3(-10063, -3455, 21), 56.0, 35, 80),
            new GrindingSpot(new Vector3(-10006, -3555, 22), 56.0, 35, 80),
        };

        public override string ToString()
        {
            return "[A][Elwynn Forest] 1 To 80 Ultimate Grinding";
        }
    }
}