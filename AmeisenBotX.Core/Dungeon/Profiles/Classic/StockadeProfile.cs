using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Dungeon.Enums;
using AmeisenBotX.Core.Dungeon.Objects;
using AmeisenBotX.Core.Jobs.Profiles;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Dungeon.Profiles.Classic
{
    public class StockadeProfile : IDungeonProfile
    {
        public string Author { get; } = "Jannis";

        public string Description { get; } = "Profile for the Dungeon in Stormwind, made for Level 24 to 31.";

        public DungeonFactionType FactionType { get; } = DungeonFactionType.Neutral;

        public int GroupSize { get; } = 5;

        public MapId MapId { get; } = MapId.StormwindStockade;

        public int MaxLevel { get; } = 31;

        public string Name { get; } = "[24-31] Stockade";

        public List<DungeonNode> Nodes { get; } = new List<DungeonNode>()
        {
            new DungeonNode(new Vector3(54, 0, -18)),
            new DungeonNode(new Vector3(61, 0, -21)),
            new DungeonNode(new Vector3(69, 1, -24)),
            new DungeonNode(new Vector3(77, 1, -26)),
            new DungeonNode(new Vector3(86, 1, -26)),
            new DungeonNode(new Vector3(95, 1, -26)),
            new DungeonNode(new Vector3(103, 1, -26)),
            new DungeonNode(new Vector3(112, 1, -26)),
            new DungeonNode(new Vector3(121, 1, -26)),
            new DungeonNode(new Vector3(129, 1, -26)),
            new DungeonNode(new Vector3(138, 1, -26)),
            new DungeonNode(new Vector3(147, 1, -26)),
            new DungeonNode(new Vector3(155, 0, -26)),
            new DungeonNode(new Vector3(164, -1, -26)),
            new DungeonNode(new Vector3(156, 1, -26)),
            new DungeonNode(new Vector3(148, 1, -26)),
            new DungeonNode(new Vector3(140, 1, -26)),
            new DungeonNode(new Vector3(131, 3, -26)),
            new DungeonNode(new Vector3(128, 11, -26)),
            new DungeonNode(new Vector3(128, 19, -28)),
            new DungeonNode(new Vector3(129, 26, -31)),
            new DungeonNode(new Vector3(129, 34, -34)),
            new DungeonNode(new Vector3(130, 43, -34)),
            new DungeonNode(new Vector3(131, 51, -34)),
            new DungeonNode(new Vector3(133, 60, -34)),
            new DungeonNode(new Vector3(135, 69, -34)),
            new DungeonNode(new Vector3(138, 76, -34)),
            new DungeonNode(new Vector3(141, 83, -34)),
            new DungeonNode(new Vector3(144, 90, -34)),
            new DungeonNode(new Vector3(147, 97, -35)),
            new DungeonNode(new Vector3(152, 105, -35)),
            new DungeonNode(new Vector3(156, 113, -35)),
            new DungeonNode(new Vector3(160, 120, -35)),
            new DungeonNode(new Vector3(164, 127, -34)),
            new DungeonNode(new Vector3(168, 134, -34)),
            new DungeonNode(new Vector3(163, 126, -34)),
            new DungeonNode(new Vector3(159, 118, -35)),
            new DungeonNode(new Vector3(154, 110, -35)),
            new DungeonNode(new Vector3(151, 103, -35)),
            new DungeonNode(new Vector3(146, 95, -35)),
            new DungeonNode(new Vector3(142, 87, -34)),
            new DungeonNode(new Vector3(139, 80, -34)),
            new DungeonNode(new Vector3(136, 73, -34)),
            new DungeonNode(new Vector3(134, 64, -34)),
            new DungeonNode(new Vector3(132, 55, -34)),
            new DungeonNode(new Vector3(131, 46, -34)),
            new DungeonNode(new Vector3(130, 38, -34)),
            new DungeonNode(new Vector3(130, 29, -32)),
            new DungeonNode(new Vector3(129, 22, -29)),
            new DungeonNode(new Vector3(129, 14, -26)),
            new DungeonNode(new Vector3(129, 6, -26)),
            new DungeonNode(new Vector3(129, -3, -26)),
            new DungeonNode(new Vector3(129, -12, -26)),
            new DungeonNode(new Vector3(128, -20, -29)),
            new DungeonNode(new Vector3(129, -27, -32)),
            new DungeonNode(new Vector3(128, -35, -34)),
            new DungeonNode(new Vector3(128, -43, -34)),
            new DungeonNode(new Vector3(126, -52, -34)),
            new DungeonNode(new Vector3(124, -60, -34)),
            new DungeonNode(new Vector3(122, -68, -34)),
            new DungeonNode(new Vector3(119, -77, -34)),
            new DungeonNode(new Vector3(116, -84, -34)),
            new DungeonNode(new Vector3(113, -91, -34)),
            new DungeonNode(new Vector3(109, -99, -35)),
            new DungeonNode(new Vector3(105, -106, -35)),
            new DungeonNode(new Vector3(101, -114, -35)),
            new DungeonNode(new Vector3(97, -121, -34)),
            new DungeonNode(new Vector3(93, -129, -34)),
            new DungeonNode(new Vector3(89, -136, -34)),
            new DungeonNode(new Vector3(94, -128, -34)),
            new DungeonNode(new Vector3(98, -120, -34)),
            new DungeonNode(new Vector3(102, -113, -35)),
            new DungeonNode(new Vector3(105, -106, -35)),
            new DungeonNode(new Vector3(109, -98, -35)),
            new DungeonNode(new Vector3(114, -90, -34)),
            new DungeonNode(new Vector3(117, -83, -34)),
            new DungeonNode(new Vector3(120, -76, -34)),
            new DungeonNode(new Vector3(123, -68, -34)),
            new DungeonNode(new Vector3(125, -60, -34)),
            new DungeonNode(new Vector3(127, -51, -34)),
            new DungeonNode(new Vector3(128, -42, -34)),
            new DungeonNode(new Vector3(129, -34, -34)),
            new DungeonNode(new Vector3(129, -25, -31)),
            new DungeonNode(new Vector3(129, -18, -28)),
            new DungeonNode(new Vector3(129, -10, -26)),
            new DungeonNode(new Vector3(129, -2, -26)),
            new DungeonNode(new Vector3(121, 0, -26)),
            new DungeonNode(new Vector3(112, 0, -26)),
            new DungeonNode(new Vector3(103, 1, -26)),
            new DungeonNode(new Vector3(95, 1, -26)),
            new DungeonNode(new Vector3(86, 1, -26)),
            new DungeonNode(new Vector3(77, 1, -26)),
            new DungeonNode(new Vector3(69, 1, -24)),
        };

        public List<string> PriorityUnits { get; } = new List<string>();

        public int RequiredItemLevel { get; } = 10;

        public int RequiredLevel { get; } = 24;

        public Vector3 WorldEntry { get; } = new Vector3(-8765, 846, 88);

        public MapId WorldEntryMapId { get; } = MapId.EasternKingdoms;
    }
}