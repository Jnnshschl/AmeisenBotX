using AmeisenBotX.Core.Engines.Grinding.Objects;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Grinding.Profiles
{
    public interface IGrindingProfile
    {
        bool RandomizeSpots { get; }

        List<GrindingSpot> Spots { get; }
    }
}