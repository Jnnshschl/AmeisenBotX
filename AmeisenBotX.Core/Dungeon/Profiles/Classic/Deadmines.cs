using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Dungeon.Enums;
using AmeisenBotX.Core.Dungeon.Objects;
using AmeisenBotX.Core.Jobs.Profiles;
using AmeisenBotX.Pathfinding.Objects;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Dungeon.Profiles.Classic
{
    public class Deadmines : IDungeonProfile
    {
        public string Author { get; } = "Jannis";

        public string Description { get; } = "Profile for the Dungeon in Westfall, made for Level 15 to 18.";

        public DungeonFactionType FactionType { get; } = DungeonFactionType.Neutral;

        public int GroupSize { get; } = 5;

        public MapId MapId { get; } = MapId.Deadmines;

        public int MaxLevel { get; } = 18;

        public string Name { get; } = "[15-18] Deadmines";

        public List<DungeonNode> Path { get; private set; } = new List<DungeonNode>()
        {
            new DungeonNode(new Vector3(-16, -383, 62), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(-34, -375, 59), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(-52, -372, 55), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(-74, -372, 55), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(-97, -378, 58), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(-120, -384, 59), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(-135, -402, 58), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(-162, -398, 56), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(-182, -418, 54), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(-193, -444, 54), DungeonNodeType.Boss),
            new DungeonNode(new Vector3(-191, -457, 54), DungeonNodeType.Door),
            new DungeonNode(new Vector3(-188, -485, 54), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(-193, -505, 53), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(-218, -497, 49), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(-243, -482, 49), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(-260, -482, 49), DungeonNodeType.Door),
            new DungeonNode(new Vector3(-278, -484, 49), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(-284, -497, 49), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(-290, -520, 50), DungeonNodeType.Boss),
            new DungeonNode(new Vector3(-290, -534, 49), DungeonNodeType.Door),
            new DungeonNode(new Vector3(-290, -558, 49), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(-304, -597, 48), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(-286, -602, 49), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(-269, -578, 50), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(-245, -579, 51), DungeonNodeType.Door),
            new DungeonNode(new Vector3(-230, -579, 51), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(-217, -556, 51), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(-192, -555, 51), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(-178, -578, 47), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(-187, -598, 38), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(-202, -606, 31), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(-229, -597, 20), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(-237, -581, 19), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(-218, -580, 21), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(-204, -592, 21), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(-222, -572, 21), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(-226, -557, 19), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(-209, -550, 19), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(-192, -553, 19), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(-179, -573, 19), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(-170, -580, 19), DungeonNodeType.Door),
            new DungeonNode(new Vector3(-154, -580, 19), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(-135, -582, 18), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(-132, -596, 17), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(-132, -614, 13), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(-107, -617, 14), DungeonNodeType.Collect, "Defias Gunpowder"),
            new DungeonNode(new Vector3(-132, -621, 13), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(-124, -637, 13), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(-106, -648, 7), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(-105, -658, 7), DungeonNodeType.Use, "Defias Gunpowder"),
            new DungeonNode(new Vector3(-98, -678, 7 ), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(-97, -691, 8), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(-97, -704, 8), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(-97, -717, 9), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(-90, -726, 9), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(-65, -731, 8), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(-53, -729, 9), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(-35, -727, 9), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(-21, -731, 8), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(-6, -742, 9), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(3, -755, 9), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(-1, -777, -10), DungeonNodeType.Boss),
            new DungeonNode(new Vector3(-20, -796, 19), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(-29, -802, 19), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(-37, -790, 19), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(-54, -782, 18), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(-76, -783, 17), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(-101, -791, 17), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(-116, -796, 17), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(-124, -791, 17), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(-101, -780, 22), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(-84, -776, 27), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(-81, -787, 26), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(-104, 790, 28), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(-102, 801, 30), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(-97, -805, 30), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(-79, 793, 39), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(-63, -789, 40), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(-41, 794, 40), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(-45, -807, 42), DungeonNodeType.Normal),
            new DungeonNode(new Vector3(-59, -812, 42), DungeonNodeType.Boss),
            new DungeonNode(new Vector3(-80, -820, 40), DungeonNodeType.Boss),
            new DungeonNode(new Vector3(-64, 821, 41), DungeonNodeType.Normal)
        };

        public int RequiredItemLevel { get; } = 10;

        public int RequiredLevel { get; } = 15;

        public Vector3 WorldEntry { get; } = new Vector3(-11208, 1680, 24);
    }
}