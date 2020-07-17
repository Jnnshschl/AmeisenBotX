using AmeisenBotX.Core.Grinding.Objects;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Grinding.Profiles
{
    public interface IGrindingProfile
    {
        bool RandomizeSpots { get; }

        List<GrindingSpot> Spots { get; }
    }
}