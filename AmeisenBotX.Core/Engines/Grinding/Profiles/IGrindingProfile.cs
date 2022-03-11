using AmeisenBotX.Core.Engines.Grinding.Objects;
using AmeisenBotX.Core.Objects;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Grinding.Profiles
{
    public interface IGrindingProfile
    {
        List<Npc> NpcsOfInterest { get; }

        List<InteractableObject> ObjectsOfInterest { get; }

        bool RandomizeSpots { get; }

        List<GrindingSpot> Spots { get; }
    }
}