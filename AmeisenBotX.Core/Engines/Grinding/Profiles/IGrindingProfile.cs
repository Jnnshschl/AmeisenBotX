using AmeisenBotX.Core.Engines.Grinding.Objects;
using AmeisenBotX.Core.Objects;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Grinding.Profiles
{
    public interface IGrindingProfile
    {
        bool RandomizeSpots { get; }

        List<Npc> NpcsOfInterest { get; }

        List<InteractableObject> ObjectsOfInterest { get; }

        List<GrindingSpot> Spots { get; }
    }
}