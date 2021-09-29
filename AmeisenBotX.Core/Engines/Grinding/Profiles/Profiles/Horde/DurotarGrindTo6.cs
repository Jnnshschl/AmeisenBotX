﻿using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Grinding.Objects;
using AmeisenBotX.Core.Objects.Mail;
using AmeisenBotX.Core.Objects.Npc;
using AmeisenBotX.Wow.Objects.Enums;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Grinding.Profiles.Profiles.Horde
{
    public class DurotarGrindTo6 : IGrindingProfile
    {
        public bool RandomizeSpots => true;

        public List<Vendor> Vendors { get; } = new()
        {
            new Vendor("Duokna", 3158,
                WowMapId.Kalimdor, WowZoneId.ValleyofTrials, new Vector3(-565, -4214, 41),
                NpcType.VendorSellBuy)
        };

        public List<Trainer> Trainers { get; } = new()
        {
            new Trainer("Ken'jai", 3707,
                WowMapId.Kalimdor, WowZoneId.ValleyofTrials, new Vector3(-617, -4202, 38),
                NpcType.ClassTrainer, NpcSubType.PriestTrainer)
        };

        public List<Mailbox> Mailboxes { get; }

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