using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Jobs.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Jobs.Profiles.Gathering
{
    public class CopperElwynnForestProfile : IMiningProfile
    {
        public bool IsCirclePath => true;

        public JobType JobType => JobType.Mining;

        public List<Vector3> Mailboxes { get; private set; } = new List<Vector3>()
        {
           new Vector3(-9465, -69, 56),
           new Vector3(-9249, -2144, 64)
        };

        public List<OreNodes> OreTypes { get; } = new List<OreNodes>()
        {
            OreNodes.Copper
        };

        public List<Vector3> Path { get; } = new List<Vector3>()
          {
              new Vector3(-8803, -1209, 100),
              new Vector3(-8808, -1204, 96),
              new Vector3(-8812, -1198, 93),
              new Vector3(-8814, -1191, 91),
              new Vector3(-8818, -1184, 91),
              new Vector3(-8822, -1178, 88),
              new Vector3(-8827, -1173, 85),
              new Vector3(-8831, -1168, 81),
              new Vector3(-8830, -1161, 78),
              new Vector3(-8823, -1157, 79),
              new Vector3(-8816, -1159, 81),
              new Vector3(-8812, -1164, 86),
              new Vector3(-8810, -1171, 90),
              new Vector3(-8809, -1178, 93),
              new Vector3(-8812, -1185, 93),
              new Vector3(-8813, -1193, 92),
              new Vector3(-8810, -1200, 95),
              new Vector3(-8806, -1206, 98),
              new Vector3(-8804, -1214, 98),
              new Vector3(-8805, -1221, 93),
              new Vector3(-8804, -1229, 91),
              new Vector3(-8809, -1236, 93),
              new Vector3(-8816, -1238, 95),
              new Vector3(-8823, -1237, 92),
              new Vector3(-8830, -1236, 88),
              new Vector3(-8836, -1233, 84),
              new Vector3(-8840, -1227, 81),
              new Vector3(-8840, -1219, 80),
              new Vector3(-8835, -1213, 80),
              new Vector3(-8829, -1208, 82),
              new Vector3(-8824, -1203, 85),
              new Vector3(-8820, -1197, 89),
              new Vector3(-8813, -1198, 93),
              new Vector3(-8809, -1204, 96),
          };
    }
}