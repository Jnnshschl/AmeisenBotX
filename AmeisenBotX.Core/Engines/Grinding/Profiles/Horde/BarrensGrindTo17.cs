using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Grinding.Objects;
using AmeisenBotX.Core.Objects;
using AmeisenBotX.Core.Objects.Enums;
using AmeisenBotX.Wow.Objects.Enums;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Grinding.Profiles.Horde
{
    public class BarrensGrindTo17 : IGrindingProfile
    {
        public List<Npc> NpcsOfInterest { get; } = new()
        {
            new Npc("Nargal Deatheye", 3479,
                WowMapId.Kalimdor, WowZoneId.TheCrossroads, new Vector3(-356, -2568, 95),
                NpcType.VendorRepair)
        };

        public List<InteractableObject> ObjectsOfInterest { get; } = new()
        {
            new InteractableObject(143982,
                WowMapId.Kalimdor, WowZoneId.RazorHill, new Vector3(-443, -2649, 95),
                InteractableObjectType.Mailbox, MailboxFactionType.Horde)
        };

        public bool RandomizeSpots => false;

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