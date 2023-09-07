﻿using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Dungeon.Enums;
using AmeisenBotX.Core.Engines.Dungeon.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Dungeon.Profiles.TBC
{
    public class TheSlavePensProfile : IDungeonProfile
    {
        public string Author { get; } = "Jannis";

        public string Description { get; } = "Profile for the Dungeon in Outland, made for Level 60 to 64.";

        public Vector3 DungeonExit { get; } = new(119, 136, -1);

        public DungeonFactionType FactionType { get; } = DungeonFactionType.Neutral;

        public int GroupSize { get; } = 5;

        public WowMapId MapId { get; } = WowMapId.TheSlavePens;

        public int MaxLevel { get; } = 64;

        public string Name { get; } = "[60-64] The Slave Pens";

        public List<IDungeonNode> Nodes { get; private set; } = new()
        {
            new DungeonNode(new(120, -132, -1)),
            new DungeonNode(new(122, -124, 0)),
            new DungeonNode(new(125, -117, -1)),
            new DungeonNode(new(123, -109, -1)),
            new DungeonNode(new(118, -103, -2)),
            new DungeonNode(new(112, -98, -2)),
            new DungeonNode(new(108, -91, -2)),
            new DungeonNode(new(104, -84, -2)),
            new DungeonNode(new(99, -78, -2)),
            new DungeonNode(new(93, -73, -2)),
            new DungeonNode(new(86, -69, -2)),
            new DungeonNode(new(79, -65, -2)),
            new DungeonNode(new(72, -61, -1)),
            new DungeonNode(new(65, -57, -1)),
            new DungeonNode(new(58, -52, -1)),
            new DungeonNode(new(51, -48, -2)),
            new DungeonNode(new(45, -43, -1)),
            new DungeonNode(new(38, -39, -1)),
            new DungeonNode(new(31, -35, -1)),
            new DungeonNode(new(24, -32, -1)),
            new DungeonNode(new(17, -28, -1)),
            new DungeonNode(new(10, -25, -1)),
            new DungeonNode(new(3, -22, -2)),
            new DungeonNode(new(-4, -18, -2)),
            new DungeonNode(new(-11, -15, -2)),
            new DungeonNode(new(-18, -12, -2)),
            new DungeonNode(new(-24, -7, -1)),
            new DungeonNode(new(-32, -6, -1)),
            new DungeonNode(new(-40, -8, -1)),
            new DungeonNode(new(-48, -9, -2)),
            new DungeonNode(new(-56, -9, -3)),
            new DungeonNode(new(-64, -8, -4)),
            new DungeonNode(new(-72, -7, -6)),
            new DungeonNode(new(-80, -6, -8)),
            new DungeonNode(new(-88, -6, -8)),
            new DungeonNode(new(-96, -6, -9)),
            new DungeonNode(new(-103, -9, -10)),
            new DungeonNode(new(-110, -13, -9)),
            new DungeonNode(new(-115, -20, -8)),
            new DungeonNode(new(-116, -28, -6)),
            new DungeonNode(new(-114, -36, -5)),
            new DungeonNode(new(-111, -43, -4)),
            new DungeonNode(new(-109, -51, -4)),
            new DungeonNode(new(-107, -59, -3)),
            new DungeonNode(new(-105, -67, -3)),
            new DungeonNode(new(-104, -75, -4)),
            new DungeonNode(new(-102, -83, -4)),
            new DungeonNode(new(-100, -91, -4)),
            new DungeonNode(new(-99, -99, -4)),
            new DungeonNode(new(-99, -107, -4)),
            new DungeonNode(new(-99, -115, -3)),
            new DungeonNode(new(-100, -123, -2)),
            new DungeonNode(new(-104, -130, -2)),
            new DungeonNode(new(-109, -136, -1)),
            new DungeonNode(new(-109, -144, -2)),
            new DungeonNode(new(-103, -149, -2)),
            new DungeonNode(new(-95, -151, -2)),
            new DungeonNode(new(-87, -153, -2)),
            new DungeonNode(new(-79, -155, -2)),
            new DungeonNode(new(-71, -158, -2)),
            new DungeonNode(new(-64, -161, -2)),
            new DungeonNode(new(-57, -165, -2)),
            new DungeonNode(new(-50, -169, -2)),
            new DungeonNode(new(-43, -172, -2)),
            new DungeonNode(new(-36, -176, -2)),
            new DungeonNode(new(-29, -181, -2)),
            new DungeonNode(new(-23, -187, -2)),
            new DungeonNode(new(-18, -193, -2)),
            new DungeonNode(new(-16, -201, -2)),
            new DungeonNode(new(-16, -209, -2)),
            new DungeonNode(new(-14, -217, -2)),
            new DungeonNode(new(-13, -225, -2)),
            new DungeonNode(new(-14, -233, -2)),
            new DungeonNode(new(-16, -241, -2)),
            new DungeonNode(new(-18, -249, -2)),
            new DungeonNode(new(-20, -257, -1)),
            new DungeonNode(new(-23, -264, -1)),
            new DungeonNode(new(-25, -272, -1)),
            new DungeonNode(new(-27, -280, -1)),
            new DungeonNode(new(-29, -288, -1)),
            new DungeonNode(new(-31, -296, -1)),
            new DungeonNode(new(-24, -299, 0)),
            new DungeonNode(new(-17, -302, 2)),
            new DungeonNode(new(-9, -304, 3)),
            new DungeonNode(new(-1, -306, 3)),
            new DungeonNode(new(7, -309, 3)),
            new DungeonNode(new(15, -311, 3)),
            new DungeonNode(new(23, -313, 3)),
            new DungeonNode(new(31, -315, 3)),
            new DungeonNode(new(39, -317, 3)),
            new DungeonNode(new(47, -319, 3)),
            new DungeonNode(new(55, -321, 3)),
            new DungeonNode(new(63, -323, 3)),
            new DungeonNode(new(71, -325, 3)),
            new DungeonNode(new(79, -327, 3)),
            new DungeonNode(new(87, -330, 3)),
            new DungeonNode(new(87, -338, 3)),
            new DungeonNode(new(81, -343, 3)),
            new DungeonNode(new(75, -348, 3)),
            new DungeonNode(new(69, -353, 3)),
            new DungeonNode(new(62, -357, 3)),
            new DungeonNode(new(55, -361, 3)),
            new DungeonNode(new(47, -364, 3)),
            new DungeonNode(new(41, -370, 3)),
            new DungeonNode(new(43, -378, 3)),
            new DungeonNode(new(50, -381, 3)),
            new DungeonNode(new(58, -381, 3)),
            new DungeonNode(new(66, -381, 5)),
            new DungeonNode(new(74, -380, 8)),
            new DungeonNode(new(81, -380, 11)),
            new DungeonNode(new(88, -380, 14)),
            new DungeonNode(new(95, -380, 18)),
            new DungeonNode(new(102, -380, 22)),
            new DungeonNode(new(109, -380, 25)),
            new DungeonNode(new(116, -381, 28)),
            new DungeonNode(new(124, -381, 30)),
            new DungeonNode(new(127, -374, 31)),
            new DungeonNode(new(124, -367, 31)),
            new DungeonNode(new(116, -365, 31)),
            new DungeonNode(new(108, -363, 32)),
            new DungeonNode(new(101, -360, 33)),
            new DungeonNode(new(93, -358, 34)),
            new DungeonNode(new(85, -356, 34)),
            new DungeonNode(new(78, -354, 36)),
            new DungeonNode(new(71, -351, 38)),
            new DungeonNode(new(63, -349, 39)),
            new DungeonNode(new(55, -347, 42)),
            new DungeonNode(new(47, -347, 45)),
            new DungeonNode(new(39, -346, 47)),
            new DungeonNode(new(32, -346, 50)),
            new DungeonNode(new(25, -345, 53)),
            new DungeonNode(new(18, -346, 56)),
            new DungeonNode(new(11, -346, 59)),
            new DungeonNode(new(4, -346, 63)),
            new DungeonNode(new(-3, -347, 66)),
            new DungeonNode(new(-10, -348, 69)),
            new DungeonNode(new(-17, -351, 72)),
            new DungeonNode(new(-24, -353, 75)),
            new DungeonNode(new(-31, -355, 77)),
            new DungeonNode(new(-38, -360, 79)),
            new DungeonNode(new(-44, -365, 80)),
            new DungeonNode(new(-50, -370, 81)),
            new DungeonNode(new(-56, -375, 81)),
            new DungeonNode(new(-63, -379, 80)),
            new DungeonNode(new(-71, -380, 79)),
            new DungeonNode(new(-79, -380, 79)),
            new DungeonNode(new(-87, -381, 79)),
            new DungeonNode(new(-95, -381, 79)),
            new DungeonNode(new(-103, -381, 80)),
            new DungeonNode(new(-111, -380, 81)),
            new DungeonNode(new(-116, -374, 81)),
            new DungeonNode(new(-121, -367, 81)),
            new DungeonNode(new(-127, -362, 80)),
            new DungeonNode(new(-134, -358, 78)),
            new DungeonNode(new(-141, -354, 76)),
            new DungeonNode(new(-148, -351, 74)),
            new DungeonNode(new(-155, -350, 71)),
            new DungeonNode(new(-162, -349, 68)),
            new DungeonNode(new(-169, -348, 65)),
            new DungeonNode(new(-176, -348, 61)),
            new DungeonNode(new(-183, -349, 57)),
            new DungeonNode(new(-188, -353, 53)),
            new DungeonNode(new(-199, -369, 7)),
            new DungeonNode(new(-203, -376, 7)),
            new DungeonNode(new(-208, -382, 8)),
            new DungeonNode(new(-216, -382, 8)),
            new DungeonNode(new(-223, -381, 4)),
            new DungeonNode(new(-231, -379, 3)),
            new DungeonNode(new(-238, -380, 6)),
            new DungeonNode(new(-245, -380, 9)),
            new DungeonNode(new(-252, -380, 12)),
            new DungeonNode(new(-259, -380, 15)),
            new DungeonNode(new(-266, -380, 19)),
            new DungeonNode(new(-273, -380, 23)),
            new DungeonNode(new(-280, -380, 26)),
            new DungeonNode(new(-287, -380, 29)),
            new DungeonNode(new(-280, -381, 26)),
            new DungeonNode(new(-273, -381, 23)),
            new DungeonNode(new(-266, -381, 19)),
            new DungeonNode(new(-259, -381, 16)),
            new DungeonNode(new(-252, -382, 12)),
            new DungeonNode(new(-245, -382, 9)),
            new DungeonNode(new(-238, -385, 6)),
            new DungeonNode(new(-231, -389, 4)),
            new DungeonNode(new(-230, -397, 3)),
            new DungeonNode(new(-236, -403, 3)),
            new DungeonNode(new(-243, -406, 3)),
            new DungeonNode(new(-250, -410, 3)),
            new DungeonNode(new(-251, -418, 3)),
            new DungeonNode(new(-253, -426, 3)),
            new DungeonNode(new(-248, -432, 3)),
            new DungeonNode(new(-240, -434, 3)),
            new DungeonNode(new(-232, -436, 3)),
            new DungeonNode(new(-224, -438, 3)),
            new DungeonNode(new(-216, -440, 3)),
            new DungeonNode(new(-208, -442, 3)),
            new DungeonNode(new(-200, -444, 3)),
            new DungeonNode(new(-192, -446, 3)),
            new DungeonNode(new(-184, -448, 3)),
            new DungeonNode(new(-176, -450, 3)),
            new DungeonNode(new(-168, -453, 3)),
            new DungeonNode(new(-161, -456, 3)),
            new DungeonNode(new(-154, -460, 2)),
            new DungeonNode(new(-147, -464, 1)),
            new DungeonNode(new(-140, -468, -1)),
            new DungeonNode(new(-133, -473, -1)),
            new DungeonNode(new(-125, -474, -1)),
            new DungeonNode(new(-117, -473, -1)),
            new DungeonNode(new(-110, -470, -1)),
            new DungeonNode(new(-102, -468, -1)),
            new DungeonNode(new(-95, -465, -1)),
            new DungeonNode(new(-88, -460, -2)),
            new DungeonNode(new(-81, -457, -2)),
            new DungeonNode(new(-73, -457, -2)),
            new DungeonNode(new(-65, -457, -2)),
            new DungeonNode(new(-57, -458, -2)),
            new DungeonNode(new(-49, -458, -2)),
            new DungeonNode(new(-41, -459, -2)),
            new DungeonNode(new(-33, -459, -2)),
            new DungeonNode(new(-25, -459, 0)),
            new DungeonNode(new(-17, -458, 2)),
            new DungeonNode(new(-9, -458, 3)),
            new DungeonNode(new(-1, -457, 3)),
            new DungeonNode(new(-8, -454, 3)),
            new DungeonNode(new(-16, -456, 2)),
            new DungeonNode(new(-24, -458, 1)),
            new DungeonNode(new(-32, -459, -1)),
            new DungeonNode(new(-40, -461, -2)),
            new DungeonNode(new(-48, -463, -2)),
            new DungeonNode(new(-56, -464, -2)),
            new DungeonNode(new(-64, -466, -2)),
            new DungeonNode(new(-72, -468, -2)),
            new DungeonNode(new(-79, -471, -2)),
            new DungeonNode(new(-82, -478, -2)),
            new DungeonNode(new(-83, -486, -2)),
            new DungeonNode(new(-84, -494, -2)),
            new DungeonNode(new(-84, -502, -2)),
            new DungeonNode(new(-82, -510, -2)),
            new DungeonNode(new(-77, -516, -2)),
            new DungeonNode(new(-72, -523, -2)),
            new DungeonNode(new(-69, -530, -2)),
            new DungeonNode(new(-67, -538, -2)),
            new DungeonNode(new(-65, -546, -2)),
            new DungeonNode(new(-65, -554, -1)),
            new DungeonNode(new(-68, -561, -1)),
            new DungeonNode(new(-71, -568, 0)),
            new DungeonNode(new(-75, -575, 1)),
            new DungeonNode(new(-80, -582, 1)),
            new DungeonNode(new(-84, -589, 3)),
            new DungeonNode(new(-90, -594, 5)),
            new DungeonNode(new(-95, -600, 7)),
            new DungeonNode(new(-97, -608, 10)),
            new DungeonNode(new(-97, -615, 13)),
            new DungeonNode(new(-98, -622, 16)),
            new DungeonNode(new(-99, -630, 18)),
            new DungeonNode(new(-100, -638, 20)),
            new DungeonNode(new(-101, -646, 19)),
            new DungeonNode(new(-100, -654, 21)),
            new DungeonNode(new(-99, -661, 25)),
            new DungeonNode(new(-98, -668, 28)),
            new DungeonNode(new(-96, -675, 31)),
            new DungeonNode(new(-94, -682, 33)),
            new DungeonNode(new(-91, -690, 35)),
            new DungeonNode(new(-89, -698, 36)),
            new DungeonNode(new(-87, -706, 37)),
            new DungeonNode(new(-85, -714, 37)),
            new DungeonNode(new(-83, -722, 37)),
            new DungeonNode(new(-82, -730, 37)),
            new DungeonNode(new(-82, -738, 37)),
            new DungeonNode(new(-89, -743, 37)),
            new DungeonNode(new(-96, -747, 37)),
            new DungeonNode(new(-103, -751, 37)),
            new DungeonNode(new(-111, -752, 37)),
            new DungeonNode(new(-119, -751, 37)),
            new DungeonNode(new(-127, -750, 38)),
            new DungeonNode(new(-135, -749, 38)),
            new DungeonNode(new(-142, -746, 38)),
            new DungeonNode(new(-149, -742, 38)),
            new DungeonNode(new(-156, -737, 38)),
            new DungeonNode(new(-163, -733, 38)),
            new DungeonNode(new(-170, -728, 38)),
            new DungeonNode(new(-177, -724, 38)),
            new DungeonNode(new(-185, -722, 38)),
            new DungeonNode(new(-193, -722, 38)),
            new DungeonNode(new(-201, -722, 37)),
            new DungeonNode(new(-209, -722, 36)),
            new DungeonNode(new(-216, -719, 36)),
            new DungeonNode(new(-224, -716, 37)),
            new DungeonNode(new(-232, -713, 37)),
            new DungeonNode(new(-240, -710, 37)),
            new DungeonNode(new(-247, -707, 37)),
            new DungeonNode(new(-254, -704, 37)),
            new DungeonNode(new(-261, -700, 37)),
            new DungeonNode(new(-268, -697, 37)),
            new DungeonNode(new(-275, -693, 37)),
            new DungeonNode(new(-282, -690, 37)),
            new DungeonNode(new(-284, -683, 35)),
            new DungeonNode(new(-282, -675, 35)),
            new DungeonNode(new(-277, -669, 35)),
            new DungeonNode(new(-270, -672, 35)),
            new DungeonNode(new(-263, -676, 35)),
            new DungeonNode(new(-257, -681, 35)),
            new DungeonNode(new(-250, -685, 35)),
            new DungeonNode(new(-244, -690, 35)),
            new DungeonNode(new(-237, -694, 35)),
            new DungeonNode(new(-230, -698, 35)),
            new DungeonNode(new(-224, -703, 35)),
            new DungeonNode(new(-217, -707, 35)),
            new DungeonNode(new(-210, -712, 36)),
            new DungeonNode(new(-204, -717, 37)),
            new DungeonNode(new(-202, -725, 37)),
            new DungeonNode(new(-199, -733, 36)),
            new DungeonNode(new(-197, -741, 36)),
            new DungeonNode(new(-195, -749, 37)),
            new DungeonNode(new(-193, -756, 40)),
            new DungeonNode(new(-190, -763, 42)),
            new DungeonNode(new(-183, -768, 43)),
            new DungeonNode(new(-181, -776, 44)),
            new DungeonNode(new(-185, -783, 44)),
            new DungeonNode(new(-189, -790, 44)),
            new DungeonNode(new(-191, -798, 44), DungeonNodeType.Use),
        };

        public List<int> PriorityUnits { get; } = new();

        public int RequiredItemLevel { get; } = 65;

        public int RequiredLevel { get; } = 60;

        public Vector3 WorldEntry { get; } = new(737, 7017, -71);

        public WowMapId WorldEntryMapId { get; } = WowMapId.Outland;
    }
}