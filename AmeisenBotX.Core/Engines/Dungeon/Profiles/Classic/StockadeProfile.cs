using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Dungeon.Enums;
using AmeisenBotX.Core.Engines.Dungeon.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Dungeon.Profiles.Classic
{
    public class StockadeProfile : IDungeonProfile
    {
        public string Author { get; } = "Jannis";

        public string Description { get; } = "Profile for the Dungeon in Stormwind, made for Level 24 to 31.";

        public Vector3 DungeonExit { get; } = new(45, 1, -16);

        public DungeonFactionType FactionType { get; } = DungeonFactionType.Neutral;

        public int GroupSize { get; } = 5;

        public WowMapId MapId { get; } = WowMapId.StormwindStockade;

        public int MaxLevel { get; } = 31;

        public string Name { get; } = "[24-31] Stockade";

        public List<IDungeonNode> Nodes { get; } = new()
        {
            new DungeonNode(new(54, 0, -18)),
            new DungeonNode(new(61, 0, -21)),
            new DungeonNode(new(69, 1, -24)),
            new DungeonNode(new(77, 1, -26)),
            new DungeonNode(new(86, 1, -26)),
            new DungeonNode(new(95, 1, -26)),
            new DungeonNode(new(103, 1, -26)),
            new DungeonNode(new(112, 1, -26)),
            new DungeonNode(new(121, 1, -26)),
            new DungeonNode(new(129, 1, -26)),
            new DungeonNode(new(138, 1, -26)),
            new DungeonNode(new(147, 1, -26)),
            new DungeonNode(new(155, 0, -26)),
            new DungeonNode(new(164, -1, -26)),
            new DungeonNode(new(156, 1, -26)),
            new DungeonNode(new(148, 1, -26)),
            new DungeonNode(new(140, 1, -26)),
            new DungeonNode(new(131, 3, -26)),
            new DungeonNode(new(128, 11, -26)),
            new DungeonNode(new(128, 19, -28)),
            new DungeonNode(new(129, 26, -31)),
            new DungeonNode(new(129, 34, -34)),
            new DungeonNode(new(130, 43, -34)),
            new DungeonNode(new(131, 51, -34)),
            new DungeonNode(new(133, 60, -34)),
            new DungeonNode(new(135, 69, -34)),
            new DungeonNode(new(138, 76, -34)),
            new DungeonNode(new(141, 83, -34)),
            new DungeonNode(new(144, 90, -34)),
            new DungeonNode(new(147, 97, -35)),
            new DungeonNode(new(152, 105, -35)),
            new DungeonNode(new(156, 113, -35)),
            new DungeonNode(new(160, 120, -35)),
            new DungeonNode(new(164, 127, -34)),
            new DungeonNode(new(168, 134, -34)),
            new DungeonNode(new(163, 126, -34)),
            new DungeonNode(new(159, 118, -35)),
            new DungeonNode(new(154, 110, -35)),
            new DungeonNode(new(151, 103, -35)),
            new DungeonNode(new(146, 95, -35)),
            new DungeonNode(new(142, 87, -34)),
            new DungeonNode(new(139, 80, -34)),
            new DungeonNode(new(136, 73, -34)),
            new DungeonNode(new(134, 64, -34)),
            new DungeonNode(new(132, 55, -34)),
            new DungeonNode(new(131, 46, -34)),
            new DungeonNode(new(130, 38, -34)),
            new DungeonNode(new(130, 29, -32)),
            new DungeonNode(new(129, 22, -29)),
            new DungeonNode(new(129, 14, -26)),
            new DungeonNode(new(129, 6, -26)),
            new DungeonNode(new(129, -3, -26)),
            new DungeonNode(new(129, -12, -26)),
            new DungeonNode(new(128, -20, -29)),
            new DungeonNode(new(129, -27, -32)),
            new DungeonNode(new(128, -35, -34)),
            new DungeonNode(new(128, -43, -34)),
            new DungeonNode(new(126, -52, -34)),
            new DungeonNode(new(124, -60, -34)),
            new DungeonNode(new(122, -68, -34)),
            new DungeonNode(new(119, -77, -34)),
            new DungeonNode(new(116, -84, -34)),
            new DungeonNode(new(113, -91, -34)),
            new DungeonNode(new(109, -99, -35)),
            new DungeonNode(new(105, -106, -35)),
            new DungeonNode(new(101, -114, -35)),
            new DungeonNode(new(97, -121, -34)),
            new DungeonNode(new(93, -129, -34)),
            new DungeonNode(new(89, -136, -34)),
            new DungeonNode(new(94, -128, -34)),
            new DungeonNode(new(98, -120, -34)),
            new DungeonNode(new(102, -113, -35)),
            new DungeonNode(new(105, -106, -35)),
            new DungeonNode(new(109, -98, -35)),
            new DungeonNode(new(114, -90, -34)),
            new DungeonNode(new(117, -83, -34)),
            new DungeonNode(new(120, -76, -34)),
            new DungeonNode(new(123, -68, -34)),
            new DungeonNode(new(125, -60, -34)),
            new DungeonNode(new(127, -51, -34)),
            new DungeonNode(new(128, -42, -34)),
            new DungeonNode(new(129, -34, -34)),
            new DungeonNode(new(129, -25, -31)),
            new DungeonNode(new(129, -18, -28)),
            new DungeonNode(new(129, -10, -26)),
            new DungeonNode(new(129, -2, -26)),
            new DungeonNode(new(121, 0, -26)),
            new DungeonNode(new(112, 0, -26)),
            new DungeonNode(new(103, 1, -26)),
            new DungeonNode(new(95, 1, -26)),
            new DungeonNode(new(86, 1, -26)),
            new DungeonNode(new(77, 1, -26)),
            new DungeonNode(new(69, 1, -24)),
        };

        public List<int> PriorityUnits { get; } = new();

        public int RequiredItemLevel { get; } = 10;

        public int RequiredLevel { get; } = 24;

        public Vector3 WorldEntry { get; } = new(-8765, 846, 88);

        public WowMapId WorldEntryMapId { get; } = WowMapId.EasternKingdoms;
    }
}