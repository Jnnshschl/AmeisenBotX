using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Fsm.States.StaticDeathRoutes
{
    public class ForgeOfSoulsDeathRoute : IStaticDeathRoute
    {
        private int CurrentNode { get; set; } = 0;

        private List<Vector3> Path { get; } = new()
        {
            new(6447, 2061, 564),
            new(6446, 2078, 562),
            new(6433, 2089, 563),
            new(6418, 2098, 566),
            new(6404, 2106, 571),
            new(6389, 2115, 575),
            new(6375, 2123, 581),
            new(6361, 2131, 588),
            new(6347, 2138, 595),
            new(6333, 2146, 602),
            new(6320, 2154, 611),
            new(6308, 2162, 620),
            new(6296, 2169, 631),
            new(6284, 2174, 643),
            new(6272, 2177, 655),
            new(6258, 2179, 666),
            new(6244, 2179, 675),
            new(6228, 2180, 682),
            new(6212, 2181, 689),
            new(6195, 2183, 694),
            new(6178, 2186, 698),
            new(6162, 2190, 702),
            new(6145, 2194, 706),
            new(6130, 2198, 711),
            new(6114, 2204, 718),
            new(6099, 2209, 724),
            new(6083, 2214, 731),
            new(6068, 2219, 737),
            new(6052, 2223, 744),
            new(6036, 2225, 750),
            new(6019, 2226, 755),
            new(6003, 2227, 761),
            new(5986, 2228, 766),
            new(5970, 2230, 771),
            new(5953, 2231, 776),
            new(5936, 2232, 781),
            new(5919, 2233, 786),
            new(5902, 2233, 789),
            new(5885, 2233, 792),
            new(5868, 2233, 796),
            new(5851, 2233, 800),
            new(5834, 2232, 804),
            new(5818, 2231, 809),
            new(5801, 2231, 814),
            new(5784, 2230, 819),
            new(5767, 2229, 822),
            new(5750, 2227, 825),
            new(5733, 2223, 827),
            new(5717, 2217, 826),
            new(5703, 2207, 821),
            new(5697, 2192, 814),
            new(5694, 2177, 807),
            new(5693, 2160, 802),
            new(5691, 2143, 799),
            new(5689, 2127, 798),
            new(5688, 2111, 798),
            new(5684, 2096, 798),
            new(5667, 2095, 798),
            new(5650, 2097, 798),
            new(5638, 2087, 798),
            new(5636, 2070, 798),
            new(5636, 2053, 798),
            new(5644, 2038, 798),
            new(5654, 2025, 798),
            new(5664, 2013, 798),
            new(5674, 2000, 798),
            new(5678, 1997, 798),
        };

        public Vector3 GetNextPoint(Vector3 playerPosition)
        {
            if (CurrentNode < Path.Count)
            {
                if (playerPosition.GetDistance(Path[CurrentNode]) < 3.0f)
                {
                    ++CurrentNode;
                    return GetNextPoint(playerPosition);
                }

                return Path[CurrentNode];
            }

            return new(0, 0, 0);
        }

        public void Init()
        {
            CurrentNode = 0;
        }

        public bool IsUseable(WowMapId mapId, Vector3 start, Vector3 end)
        {
            return mapId == WowMapId.Northrend && ((start.GetDistance(Path[0]) < 10.0f && end.GetDistance(Path[^1]) < 10.0f) || end.GetDistance(new(5670, 2003, -100000)) < 16.0f);
        }
    }
}