using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Grinding.Objects;
using AmeisenBotX.Core.Objects;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Grinding.Profiles.Horde
{
    public class DurotarGrindTo11 : IGrindingProfile
    {
        public bool RandomizeSpots => false;

        public List<Npc> NpcsOfInterest { get; }

        public List<InteractableObject> ObjectsOfInterest { get; }

        public List<GrindingSpot> Spots { get; } = new()
        {
            // scorpids/boars
            new GrindingSpot(new Vector3(-678, -4649, 37), 55.0f, 5, 11),
            new GrindingSpot(new Vector3(-756, -4645, 41), 55.0f, 5, 11),
            new GrindingSpot(new Vector3(-762, -4732, 32), 55.0f, 5, 11),
            // centaurs
            new GrindingSpot(new Vector3(-960, -4765, 14), 55.0f, 5, 11),
        };

        public override string ToString()
        {
            return "[H][Durotar] 5 To 11 Grinding";
        }
    }
}