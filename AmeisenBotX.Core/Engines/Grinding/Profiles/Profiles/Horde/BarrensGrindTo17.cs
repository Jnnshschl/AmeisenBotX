using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Grinding.Objects;
using AmeisenBotX.Core.Engines.Npc;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Grinding.Profiles.Profiles.Horde
{
    public class BarrensGrindTo17 : IGrindingProfile
    {
        public bool RandomizeSpots { get; } = false;

        public List<Vendor> Vendors { get; } = new()
        {
            new Vendor("Nargal Deatheye", 3479, 1, new Vector3(-356.99f, -2568.86f, 95.78f), NpcType.VendorRepair)
        };

        public List<Trainer> Trainers { get; }

        public List<GrindingSpot> Spots { get; } = new()
        {
            new GrindingSpot(new Vector3(-314, -2712, 93), 50.0f, 11, 17),
            new GrindingSpot(new Vector3(-310, -2821, 92), 50.0f, 11, 17),
            new GrindingSpot(new Vector3(-181, -2905, 92), 50.0f, 11, 17),
            new GrindingSpot(new Vector3(-112, -2972, 91), 50.0f, 11, 17),
            new GrindingSpot(new Vector3(3, -3090, 91), 50.0f, 11, 17),
            new GrindingSpot(new Vector3(-9, -3201, 91), 50.0f, 11, 17)
        };

        public override string ToString()
        {
            return "[H][Durotar] 14 To 17 Grinding";
        }
    }
}