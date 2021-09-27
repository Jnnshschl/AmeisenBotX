using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Grinding.Objects;
using AmeisenBotX.Core.Objects.Mail;
using AmeisenBotX.Core.Objects.Npc;
using AmeisenBotX.Wow.Objects.Enums;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Grinding.Profiles.Profiles.Horde
{
    public class DurotarGrindTo14 : IGrindingProfile
    {
        public bool RandomizeSpots => false;

        public List<Vendor> Vendors { get; } = new()
        {
            new Vendor("Wuark", 3167,
                WowMapId.Kalimdor, WowZoneId.RazorHill, new Vector3(358.12f, -4706.73f, 14.39f),
                NpcType.VendorRepair)
        };

        public List<Trainer> Trainers { get; }

        public List<Mailbox> Mailboxes { get; } = new()
        {
            new Mailbox(WowMapId.Kalimdor, WowZoneId.RazorHill, new Vector3(322.40f, -4706.9f, 14.68f),
                MailboxFactionType.Horde)
        };

        public List<GrindingSpot> Spots { get; } = new()
        {
            // razormane/scorpids/reptiles
            new GrindingSpot(new Vector3(393, -4312, 25), 50.0f, 6, 14),
            new GrindingSpot(new Vector3(377, -4236, 24), 50.0f, 6, 14),
            new GrindingSpot(new Vector3(445, -4143, 27), 50.0f, 6, 14),
            new GrindingSpot(new Vector3(533, -4188, 17), 50.0f, 6, 14),
            new GrindingSpot(new Vector3(504, -4300, 21), 50.0f, 6, 14)
        };

        public override string ToString()
        {
            return "[H][Durotar] 10 To 14 Grinding";
        }
    }
}
