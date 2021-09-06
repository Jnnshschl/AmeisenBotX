using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Grinding.Objects;
using AmeisenBotX.Core.Engines.Npc;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Grinding.Profiles.Profiles.Horde
{
    public class DurotarGrindTo6 : IGrindingProfile
    {
        public bool RandomizeSpots { get; } = true;

        public List<Vendor> Vendors { get; } = new()
        {
            new Vendor("Duokna", 3158, 1, new Vector3(-565.428f, -4214.2f, 41.59f), NpcType.VendorSellBuy)
        };

        public List<Trainer> Trainers { get; }

        public List<GrindingSpot> Spots { get; } = new()
        {
            // pigs
            new GrindingSpot(new Vector3(-546, -4308, 38), 40.0f, 1, 3),
            new GrindingSpot(new Vector3(-450, -4258, 48), 40.0f, 1, 3),
            // scorpids
            new GrindingSpot(new Vector3(-435, -4154, 52), 48.0f, 2, 7),
            new GrindingSpot(new Vector3(-379, -4096, 49), 48.0f, 2, 7),
            new GrindingSpot(new Vector3(-399, -4116, 50), 48.0f, 2, 7)
        };

        public override string ToString()
        {
            return "[H][Durotar] 1 To 6 Grinding";
        }
    }
}