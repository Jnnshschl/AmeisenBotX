using AmeisenBotX.Core.Engines.Grinding.Objects;
using System.Collections.Generic;
using AmeisenBotX.Core.Engines.Npc;

namespace AmeisenBotX.Core.Engines.Grinding.Profiles
{
    public interface IGrindingProfile
    {
        bool RandomizeSpots { get; }

        List<Vendor> Vendors { get; }

        List<Trainer> Trainers { get; }

        List<GrindingSpot> Spots { get; }
    }
}