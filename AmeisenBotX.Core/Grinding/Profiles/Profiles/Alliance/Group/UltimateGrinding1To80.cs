using AmeisenBotX.Core.Grinding.Objects;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Grinding.Profiles.Profiles.Alliance.Group
{
    public class UltimateGrinding1To80 : IGrindingProfile
    {
        public bool RandomizeSpots { get; } = true;

        public List<GrindingSpot> Spots { get; } = new()
        {
            new(new(-9039, -265, 74), 32.0, 1, 3),
            new(new(-8890, -266, 79), 36.0, 1, 3),
            new(new(-8952, -393, 70), 42.0, 3, 5),
            new(new(-9069, -371, 73), 50.0, 3, 5),
            new(new(-9155, 70, 77), 76.0, 5, 8),
            new(new(-9453, 470, 53), 64.0, 8, 11),
            new(new(-10014, 653, 37), 76.0, 9, 12),
            new(new(-10238, 967, 38), 56.0, 11, 13),
            new(new(-10498, 1326, 43), 48.0, 12, 14),
            new(new(-11023, 1494, 43), 100.0, 13, 16),
            new(new(-11080, 944, 38), 100.0, 17, 18),
            new(new(-11164, 721, 36), 64.0, 17, 18),
            new(new(-11112, 609, 37), 64.0, 17, 18),
            new(new(-10706, 635, 34), 64.0, 19, 20),
            new(new(-10569, 545, 31), 64.0, 19, 20),
            new(new(-10549, 415, 36), 128.0, 21, 26),
            new(new(-10556, 231, 30), 128.0, 21, 26),
            new(new(-11039, -228, 15), 76.0, 27, 29),
            new(new(-11074, -513, 32), 56.0, 27, 29),
            new(new(-10079, -3366, 20), 64.0, 30, 35),
            new(new(-11071, -3455, 21), 56.0, 30, 35),
            new(new(-10151, -3497, 23), 56.0, 30, 35),
            new(new(-10063, -3455, 21), 56.0, 35, 80),
            new(new(-10006, -3555, 22), 56.0, 35, 80),
        };

        public override string ToString()
        {
            return "[A][Elwynn Forest] 1 To 80 Ultimate Grinding";
        }
    }
}