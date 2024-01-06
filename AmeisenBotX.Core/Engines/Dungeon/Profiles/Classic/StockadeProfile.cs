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

        public List<DungeonNode> Nodes { get; } =
        [
            new(new(54, 0, -18)),
            new(new(61, 0, -21)),
            new(new(69, 1, -24)),
            new(new(77, 1, -26)),
            new(new(86, 1, -26)),
            new(new(95, 1, -26)),
            new(new(103, 1, -26)),
            new(new(112, 1, -26)),
            new(new(121, 1, -26)),
            new(new(129, 1, -26)),
            new(new(138, 1, -26)),
            new(new(147, 1, -26)),
            new(new(155, 0, -26)),
            new(new(164, -1, -26)),
            new(new(156, 1, -26)),
            new(new(148, 1, -26)),
            new(new(140, 1, -26)),
            new(new(131, 3, -26)),
            new(new(128, 11, -26)),
            new(new(128, 19, -28)),
            new(new(129, 26, -31)),
            new(new(129, 34, -34)),
            new(new(130, 43, -34)),
            new(new(131, 51, -34)),
            new(new(133, 60, -34)),
            new(new(135, 69, -34)),
            new(new(138, 76, -34)),
            new(new(141, 83, -34)),
            new(new(144, 90, -34)),
            new(new(147, 97, -35)),
            new(new(152, 105, -35)),
            new(new(156, 113, -35)),
            new(new(160, 120, -35)),
            new(new(164, 127, -34)),
            new(new(168, 134, -34)),
            new(new(163, 126, -34)),
            new(new(159, 118, -35)),
            new(new(154, 110, -35)),
            new(new(151, 103, -35)),
            new(new(146, 95, -35)),
            new(new(142, 87, -34)),
            new(new(139, 80, -34)),
            new(new(136, 73, -34)),
            new(new(134, 64, -34)),
            new(new(132, 55, -34)),
            new(new(131, 46, -34)),
            new(new(130, 38, -34)),
            new(new(130, 29, -32)),
            new(new(129, 22, -29)),
            new(new(129, 14, -26)),
            new(new(129, 6, -26)),
            new(new(129, -3, -26)),
            new(new(129, -12, -26)),
            new(new(128, -20, -29)),
            new(new(129, -27, -32)),
            new(new(128, -35, -34)),
            new(new(128, -43, -34)),
            new(new(126, -52, -34)),
            new(new(124, -60, -34)),
            new(new(122, -68, -34)),
            new(new(119, -77, -34)),
            new(new(116, -84, -34)),
            new(new(113, -91, -34)),
            new(new(109, -99, -35)),
            new(new(105, -106, -35)),
            new(new(101, -114, -35)),
            new(new(97, -121, -34)),
            new(new(93, -129, -34)),
            new(new(89, -136, -34)),
            new(new(94, -128, -34)),
            new(new(98, -120, -34)),
            new(new(102, -113, -35)),
            new(new(105, -106, -35)),
            new(new(109, -98, -35)),
            new(new(114, -90, -34)),
            new(new(117, -83, -34)),
            new(new(120, -76, -34)),
            new(new(123, -68, -34)),
            new(new(125, -60, -34)),
            new(new(127, -51, -34)),
            new(new(128, -42, -34)),
            new(new(129, -34, -34)),
            new(new(129, -25, -31)),
            new(new(129, -18, -28)),
            new(new(129, -10, -26)),
            new(new(129, -2, -26)),
            new(new(121, 0, -26)),
            new(new(112, 0, -26)),
            new(new(103, 1, -26)),
            new(new(95, 1, -26)),
            new(new(86, 1, -26)),
            new(new(77, 1, -26)),
            new(new(69, 1, -24)),
        ];

        public List<int> PriorityUnits { get; } = [];

        public int RequiredItemLevel { get; } = 10;

        public int RequiredLevel { get; } = 24;

        public Vector3 WorldEntry { get; } = new(-8765, 846, 88);

        public WowMapId WorldEntryMapId { get; } = WowMapId.EasternKingdoms;
    }
}