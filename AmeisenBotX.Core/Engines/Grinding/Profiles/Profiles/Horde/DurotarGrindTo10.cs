using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Grinding.Objects;
using AmeisenBotX.Core.Engines.Npc;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Grinding.Profiles.Profiles.Horde
{
    public class DurotarGrindTo10 : IGrindingProfile
    {
        public bool RandomizeSpots { get; } = true;

        public List<Vendor> Vendors { get; } = new()
        {
            new Vendor("Trayexir", 10369, 1, new Vector3(-769.15f, -4948.53f, 22.84f), NpcType.VendorRepair)
        };

        public List<Trainer> Trainers { get; }

        public List<GrindingSpot> Spots { get; } = new()
        {
            // scorpids/boars
            new GrindingSpot(new Vector3(-678, -4649, 37), 55.0f, 5, 10),
            new GrindingSpot(new Vector3(-756, -4645, 41), 55.0f, 5, 10),
            new GrindingSpot(new Vector3(-762, -4732, 32), 55.0f, 5, 10),
            // centaurs
            new GrindingSpot(new Vector3(-960, -4765, 14), 55.0f, 5, 10),
        };

        public override string ToString()
        {
            return "[H][Durotar] 5 To 10 Grinding";
        }
    }
}