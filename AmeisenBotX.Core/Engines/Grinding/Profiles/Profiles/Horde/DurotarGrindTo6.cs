using AmeisenBotX.Core.Engines.Grinding.Objects;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Grinding.Profiles.Profiles.Horde
{
    public class DurotarGrindTo6 : IGrindingProfile
    {
        public bool RandomizeSpots { get; }

        public List<GrindingSpot> Spots { get; } = new()
        {
            // pigs
            new(new(-546, -4308, 38), 35.0f, 1, 3),
            new(new(-450, -4258, 48), 36.0f, 1, 3),
            // scorpids
            new(new(-435, -4154, 52), 42.0f, 2, 4),
            new(new(-379, -4096, 49), 50.0f, 2, 4),
        };

        public override string ToString()
        {
            return "[H][Durotar] 1 To 6 Grinding";
        }
    }
}