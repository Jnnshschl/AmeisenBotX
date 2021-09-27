using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Grinding.Objects;
using AmeisenBotX.Core.Objects.Mail;
using AmeisenBotX.Core.Objects.Npc;
using AmeisenBotX.Wow.Objects.Enums;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Grinding.Profiles.Profiles.Horde
{
    public class DurotarGrindTo11 : IGrindingProfile
    {
        public bool RandomizeSpots => false;

        public List<Vendor> Vendors { get; } = new()
        {
            new Vendor("Trayexir", 10369,
                WowMapId.Kalimdor, WowZoneId.SenjinVillage, new Vector3(-769.15f, -4948.53f, 22.84f),
                NpcType.VendorRepair)
        };

        public List<Trainer> Trainers { get; }

        public List<Mailbox> Mailboxes { get; }

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