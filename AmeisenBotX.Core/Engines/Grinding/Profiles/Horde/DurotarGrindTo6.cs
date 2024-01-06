using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Grinding.Objects;
using AmeisenBotX.Core.Objects;
using AmeisenBotX.Core.Objects.Enums;
using AmeisenBotX.Wow.Objects.Enums;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Grinding.Profiles.Horde
{
    public class DurotarGrindTo6 : IGrindingProfile
    {
        public List<Npc> NpcsOfInterest { get; } =
        [
            new Npc("Duokna", 3158,
                WowMapId.Kalimdor, WowZoneId.ValleyofTrials, new Vector3(-565, -4214, 41),
                NpcType.VendorSellBuy),

            new Npc("Ken'jai", 3707,
                WowMapId.Kalimdor, WowZoneId.ValleyofTrials, new Vector3(-617, -4202, 38),
                NpcType.ClassTrainer, NpcSubType.PriestTrainer),
            new Npc("Shikrik", 3157,
                WowMapId.Kalimdor, WowZoneId.ValleyofTrials, new Vector3(-623, -4203, 38),
                NpcType.ClassTrainer, NpcSubType.ShamanTrainer),
            new Npc("Frang", 3153,
                WowMapId.Kalimdor, WowZoneId.ValleyofTrials, new Vector3(-639, -4230, 38),
                NpcType.ClassTrainer, NpcSubType.WarriorTrainer),
            new Npc("Mai'ah", 5884,
                WowMapId.Kalimdor, WowZoneId.ValleyofTrials, new Vector3(-625, -4210, 38),
                NpcType.ClassTrainer, NpcSubType.MageTrainer)
        ];

        public List<InteractableObject> ObjectsOfInterest { get; } =
        [
            new InteractableObject(3084,
                WowMapId.Kalimdor, WowZoneId.ValleyofTrials, new Vector3(-602, -4250, 37),
                InteractableObjectType.Fire)
        ];

        public bool RandomizeSpots => true;

        public List<GrindingSpot> Spots { get; } =
        [
            // pigs
            new GrindingSpot(new Vector3(-546, -4308, 38), 45.0f, 1, 3),
            new GrindingSpot(new Vector3(-450, -4258, 48), 45.0f, 1, 3),
            // scorpids
            new GrindingSpot(new Vector3(-435, -4154, 52), 55.0f, 2, 7),
            new GrindingSpot(new Vector3(-379, -4096, 49), 55.0f, 2, 7),
            new GrindingSpot(new Vector3(-399, -4116, 50), 55.0f, 2, 7),
            new GrindingSpot(new Vector3(-284, -4179, 51), 55.0f, 2, 7),
        ];

        public override string ToString()
        {
            return "[H][Durotar] 1 To 6 Grinding";
        }
    }
}