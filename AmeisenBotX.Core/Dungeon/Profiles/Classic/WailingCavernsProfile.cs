﻿using AmeisenBotX.Common.Math;
using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Core.Dungeon.Enums;
using AmeisenBotX.Core.Dungeon.Objects;
using AmeisenBotX.Core.Jobs.Profiles;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Dungeon.Profiles.Classic
{
    public class WailingCavernsProfile : IDungeonProfile
    {
        public string Author { get; } = "Jannis";

        public string Description { get; } = "Profile for the Dungeon in The Barrens, made for Level 17 to 24.";

        public DungeonFactionType FactionType { get; } = DungeonFactionType.Neutral;

        public int GroupSize { get; } = 5;

        public WowMapId MapId { get; } = WowMapId.WailingCaverns;

        public int MaxLevel { get; } = 24;

        public string Name { get; } = "[17-24] Wailing Caverns";

        public List<DungeonNode> Nodes { get; } = new()
        {
            new(new(-163, 133, -74)),
            new(new(-156, 129, -75)),
            new(new(-148, 129, -76)),
            new(new(-140, 129, -77)),
            new(new(-131, 131, -78)),
            new(new(-123, 135, -79)),
            new(new(-117, 140, -80)),
            new(new(-114, 147, -81)),
            new(new(-111, 154, -80)),
            new(new(-111, 163, -80)),
            new(new(-110, 171, -79)),
            new(new(-110, 180, -80)),
            new(new(-110, 189, -80)),
            new(new(-109, 197, -81)),
            new(new(-109, 206, -83)),
            new(new(-110, 215, -86)),
            new(new(-109, 223, -88)),
            new(new(-106, 230, -90)),
            new(new(-98, 229, -91)),
            new(new(-89, 227, -92)),
            new(new(-82, 222, -94)),
            new(new(-76, 217, -94)),
            new(new(-70, 212, -93)),
            new(new(-64, 207, -93)),
            new(new(-55, 206, -96)),
            new(new(-46, 208, -96)),
            new(new(-40, 214, -96)),
            new(new(-36, 222, -96)),
            new(new(-34, 231, -94)),
            new(new(-35, 240, -93)),
            new(new(-40, 248, -93)),
            new(new(-45, 254, -93)),
            new(new(-50, 261, -93)),
            new(new(-53, 268, -93)),
            new(new(-55, 276, -93)),
            new(new(-58, 283, -93)),
            new(new(-55, 290, -92)),
            new(new(-49, 296, -91)),
            new(new(-43, 302, -90)),
            new(new(-36, 308, -90)),
            new(new(-29, 311, -89)),
            new(new(-20, 311, -89)),
            new(new(-11, 311, -89)),
            new(new(-4, 308, -88)),
            new(new(3, 305, -88)),
            new(new(10, 301, -87)),
            new(new(14, 294, -88)),
            new(new(17, 287, -88)),
            new(new(23, 281, -88)),
            new(new(28, 274, -87)),
            new(new(30, 266, -87)),
            new(new(30, 257, -88)),
            new(new(26, 250, -87)),
            new(new(23, 243, -86)),
            new(new(19, 235, -85)),
            new(new(16, 228, -84)),
            new(new(15, 219, -84)),
            new(new(15, 211, -85)),
            new(new(15, 202, -85)),
            new(new(12, 195, -85)),
            new(new(10, 186, -85)),
            new(new(12, 177, -87)),
            new(new(14, 168, -87)),
            new(new(14, 159, -88)),
            new(new(11, 152, -89)),
            new(new(6, 146, -89)),
            new(new(1, 139, -89)),
            new(new(-4, 132, -90)),
            new(new(-9, 125, -90)),
            new(new(-13, 118, -90)),
            new(new(-18, 110, -90)),
            new(new(-23, 103, -90)),
            new(new(-31, 104, -89)),
            new(new(-37, 109, -88)),
            new(new(-44, 113, -88)),
            new(new(-50, 118, -89)),
            new(new(-56, 124, -90)),
            new(new(-59, 131, -90)),
            new(new(-55, 139, -92)),
            new(new(-50, 146, -93)),
            new(new(-45, 152, -94)),
            new(new(-41, 160, -95)),
            new(new(-36, 168, -96)),
            new(new(-34, 177, -96)),
            new(new(-33, 186, -96)),
            new(new(-31, 195, -96)),
            new(new(-27, 202, -98)),
            new(new(-22, 209, -100)),
            new(new(-19, 216, -102)),
            new(new(-18, 224, -104)),
            new(new(-16, 233, -105)),
            new(new(-16, 242, -105)),
            new(new(-16, 250, -106)),
            new(new(-16, 259, -106)),
            new(new(-18, 267, -106)),
            new(new(-22, 274, -106)),
            new(new(-25, 281, -106)),
            new(new(-29, 288, -106)),
            new(new(-33, 296, -106)),
            new(new(-39, 301, -106)),
            new(new(-46, 307, -106)),
            new(new(-54, 312, -106)),
            new(new(-61, 315, -107)),
            new(new(-68, 318, -107)),
            new(new(-74, 324, -106)),
            new(new(-77, 331, -106)),
            new(new(-77, 339, -106)),
            new(new(-74, 348, -106)),
            new(new(-72, 357, -106)),
            new(new(-70, 366, -106)),
            new(new(-69, 375, -105)),
            new(new(-68, 384, -106)),
            new(new(-67, 392, -107)),
            new(new(-66, 401, -107)),
            new(new(-63, 408, -107)),
            new(new(-60, 415, -107)),
            new(new(-56, 422, -106)),
            new(new(-51, 429, -106)),
            new(new(-46, 435, -106)),
            new(new(-41, 442, -105)),
            new(new(-35, 448, -104)),
            new(new(-30, 453, -101)),
            new(new(-24, 458, -99)),
            new(new(-16, 463, -97)),
            new(new(-10, 468, -94)),
            new(new(-3, 471, -91)),
            new(new(6, 473, -89)),
            new(new(14, 475, -88)),
            new(new(19, 469, -88)),
            new(new(24, 463, -87)),
            new(new(29, 457, -86)),
            new(new(30, 449, -86)),
            new(new(23, 447, -84)),
            new(new(16, 449, -81)),
            new(new(9, 451, -78)),
            new(new(1, 450, -76)),
            new(new(1, 442, -74)),
            new(new(5, 434, -72)),
            new(new(1, 427, -69)),
            new(new(-5, 423, -66)),
            new(new(-14, 422, -64)),
            new(new(-21, 421, -61)),
            new(new(-28, 417, -61)),
            new(new(-33, 410, -60)),
            new(new(-33, 402, -59)),
            new(new(-26, 397, -60)),
            new(new(-17, 396, -60)),
            new(new(-9, 396, -60)),
            new(new(0, 396, -60)),
            new(new(9, 396, -60)),
            new(new(18, 397, -59)),
            new(new(26, 397, -59)),
            new(new(35, 399, -60)),
            new(new(44, 399, -61)),
            new(new(52, 397, -63)),
            new(new(59, 400, -64)),
            new(new(62, 407, -64)),
            new(new(65, 415, -64)),
            new(new(67, 423, -64)),
            new(new(67, 431, -64)),
            new(new(66, 439, -64)),
            new(new(63, 446, -64)),
            new(new(59, 453, -65)),
            new(new(55, 460, -65)),
            new(new(51, 467, -65)),
            new(new(47, 474, -65)),
            new(new(44, 481, -66)),
            new(new(42, 488, -64)),
            new(new(39, 495, -63)),
            new(new(34, 501, -61)),
            new(new(28, 507, -59)),
            new(new(21, 510, -58)),
            new(new(13, 510, -57)),
            new(new(4, 508, -57)),
            new(new(-3, 504, -56)),
            new(new(-10, 500, -56)),
            new(new(-16, 495, -55)),
            new(new(-22, 490, -55)),
            new(new(-28, 485, -54)),
            new(new(-34, 480, -54)),
            new(new(-43, 477, -55)),
            new(new(-50, 480, -56)),
            new(new(-56, 485, -58)),
            new(new(-64, 485, -60)),
            new(new(-71, 481, -63)),
            new(new(-78, 476, -66)),
            new(new(-84, 471, -68)),
            new(new(-90, 466, -70)),
            new(new(-95, 460, -71)),
            new(new(-101, 455, -72)),
            new(new(-107, 448, -73)),
            new(new(-112, 441, -73)),
            new(new(-116, 434, -73)),
            new(new(-118, 425, -73)),
            new(new(-121, 418, -73)),
            new(new(-128, 412, -73)),
            new(new(-137, 411, -73)),
            new(new(-145, 414, -73)),
            new(new(-138, 417, -73)),
            new(new(-130, 417, -73)),
            new(new(-121, 417, -73)),
            new(new(-113, 415, -74)),
            new(new(-109, 408, -75)),
            new(new(-102, 403, -78)),
            new(new(-102, 407, -81)),
            new(new(-104, 421, -90)),
            new(new(-103, 422, -91)),
            new(new(-75, 425, -105)),
            new(new(-74, 417, -107)),
            new(new(-72, 408, -107)),
            new(new(-71, 399, -107)),
            new(new(-69, 390, -107)),
            new(new(-69, 382, -106)),
            new(new(-69, 374, -105)),
            new(new(-70, 365, -106)),
            new(new(-71, 356, -106)),
            new(new(-73, 347, -106)),
            new(new(-75, 339, -106)),
            new(new(-78, 330, -106)),
            new(new(-75, 323, -106)),
            new(new(-68, 318, -107)),
            new(new(-61, 315, -107)),
            new(new(-54, 311, -106)),
            new(new(-47, 307, -106)),
            new(new(-40, 302, -106)),
            new(new(-35, 296, -106)),
            new(new(-30, 290, -106)),
            new(new(-25, 284, -106)),
            new(new(-20, 277, -106)),
            new(new(-17, 269, -106)),
            new(new(-14, 262, -106)),
            new(new(-12, 253, -106)),
            new(new(-11, 244, -106)),
            new(new(-9, 235, -106)),
            new(new(-8, 226, -106)),
            new(new(-7, 217, -106)),
            new(new(-7, 209, -106)),
            new(new(-8, 200, -106)),
            new(new(-8, 192, -106)),
            new(new(-8, 183, -106)),
            new(new(-9, 174, -106)),
            new(new(-10, 166, -106)),
            new(new(-12, 158, -106)),
            new(new(-15, 150, -106)),
            new(new(-19, 143, -106)),
            new(new(-24, 136, -106)),
            new(new(-29, 129, -106)),
            new(new(-33, 122, -106)),
            new(new(-38, 116, -106)),
            new(new(-43, 109, -106)),
            new(new(-49, 104, -106)),
            new(new(-54, 98, -106)),
            new(new(-60, 93, -106)),
            new(new(-66, 86, -106)),
            new(new(-72, 81, -106)),
            new(new(-78, 75, -106)),
            new(new(-85, 70, -106)),
            new(new(-93, 66, -106)),
            new(new(-102, 67, -106)),
            new(new(-111, 70, -106)),
            new(new(-120, 71, -106)),
            new(new(-129, 72, -106)),
            new(new(-137, 70, -106)),
            new(new(-145, 68, -106)),
            new(new(-152, 64, -106)),
            new(new(-157, 58, -106)),
            new(new(-165, 53, -106)),
            new(new(-173, 50, -106)),
            new(new(-180, 47, -106)),
            new(new(-186, 42, -105)),
            new(new(-191, 36, -104)),
            new(new(-196, 28, -105)),
            new(new(-201, 21, -106)),
            new(new(-206, 15, -107)),
            new(new(-210, 8, -106)),
            new(new(-216, 3, -106)),
            new(new(-223, 0, -106)),
            new(new(-230, -4, -106)),
            new(new(-238, -6, -106)),
            new(new(-246, -7, -106)),
            new(new(-254, -9, -106)),
            new(new(-262, -10, -106)),
            new(new(-270, -12, -106)),
            new(new(-279, -12, -106)),
            new(new(-286, -8, -105)),
            new(new(-293, -5, -106)),
            new(new(-300, 1, -106)),
            new(new(-306, 6, -106)),
            new(new(-313, 9, -106)),
            new(new(-320, 12, -106)),
            new(new(-327, 16, -105)),
            new(new(-335, 21, -103)),
            new(new(-341, 28, -100)),
            new(new(-342, 36, -99)),
            new(new(-339, 43, -99)),
            new(new(-335, 50, -98)),
            new(new(-327, 52, -97)),
            new(new(-319, 51, -96)),
            new(new(-311, 50, -94)),
            new(new(-303, 48, -93)),
            new(new(-296, 44, -92)),
            new(new(-288, 39, -90)),
            new(new(-280, 35, -89)),
            new(new(-271, 36, -88)),
            new(new(-262, 38, -87)),
            new(new(-253, 40, -86)),
            new(new(-244, 41, -84)),
            new(new(-235, 43, -83)),
            new(new(-226, 46, -82)),
            new(new(-219, 51, -82)),
            new(new(-213, 56, -82)),
            new(new(-208, 62, -82)),
            new(new(-203, 69, -82)),
            new(new(-197, 74, -82)),
            new(new(-191, 80, -81)),
            new(new(-184, 85, -79)),
            new(new(-177, 88, -78)),
            new(new(-170, 85, -77)),
            new(new(-164, 80, -76)),
            new(new(-159, 73, -76)),
            new(new(-155, 66, -76)),
            new(new(-151, 59, -76)),
            new(new(-147, 52, -76)),
            new(new(-143, 45, -75)),
            new(new(-141, 37, -75)),
            new(new(-140, 28, -75)),
            new(new(-141, 19, -74)),
            new(new(-143, 10, -74)),
            new(new(-145, 1, -75)),
            new(new(-148, -8, -76)),
            new(new(-150, -16, -78)),
            new(new(-151, -24, -79)),
            new(new(-153, -32, -81)),
            new(new(-155, -40, -80)),
            new(new(-157, -48, -78)),
            new(new(-162, -56, -76)),
            new(new(-166, -62, -73)),
            new(new(-170, -68, -70)),
            new(new(-173, -76, -68)),
            new(new(-173, -85, -68)),
            new(new(-168, -91, -67)),
            new(new(-160, -95, -67)),
            new(new(-153, -98, -67)),
            new(new(-146, -101, -69)),
            new(new(-138, -102, -70)),
            new(new(-129, -101, -71)),
            new(new(-120, -101, -73)),
            new(new(-115, -94, -73)),
            new(new(-114, -85, -72)),
            new(new(-111, -78, -71)),
            new(new(-106, -72, -70)),
            new(new(-100, -66, -68)),
            new(new(-94, -61, -66)),
            new(new(-87, -57, -64)),
            new(new(-80, -54, -61)),
            new(new(-73, -51, -62)),
            new(new(-67, -46, -62)),
            new(new(-61, -39, -62)),
            new(new(-53, -40, -63)),
            new(new(-46, -43, -64)),
            new(new(-39, -46, -65)),
            new(new(-32, -50, -66)),
            new(new(-27, -58, -67)),
            new(new(-25, -67, -67)),
            new(new(-25, -76, -67)),
            new(new(-25, -84, -69)),
            new(new(-27, -93, -70)),
            new(new(-30, -101, -71)),
            new(new(-31, -109, -71)),
            new(new(-29, -117, -71)),
            new(new(-24, -124, -72)),
            new(new(-16, -129, -74)),
            new(new(-8, -133, -75)),
            new(new(-1, -137, -76)),
            new(new(7, -141, -77)),
            new(new(14, -146, -78)),
            new(new(21, -151, -79)),
            new(new(27, -157, -80)),
            new(new(29, -164, -82)),
            new(new(27, -172, -83)),
            new(new(22, -179, -83)),
            new(new(16, -184, -83)),
            new(new(8, -188, -82)),
            new(new(2, -193, -81)),
            new(new(-3, -201, -80)),
            new(new(-2, -209, -79)),
            new(new(1, -217, -77)),
            new(new(2, -225, -76)),
            new(new(-4, -230, -73)),
            new(new(-10, -236, -71)),
            new(new(-15, -243, -70)),
            new(new(-18, -252, -69)),
            new(new(-10, -252, -69)),
            new(new(-1, -250, -71)),
            new(new(6, -247, -72)),
            new(new(13, -244, -75)),
            new(new(20, -241, -78)),
            new(new(27, -238, -79)),
            new(new(35, -240, -80)),
            new(new(27, -242, -79)),
            new(new(20, -239, -78)),
            new(new(12, -234, -77)),
            new(new(5, -229, -76)),
            new(new(-3, -229, -74)),
            new(new(-11, -233, -72)),
            new(new(-18, -237, -70)),
            new(new(-26, -240, -69)),
            new(new(-35, -241, -68)),
            new(new(-43, -241, -68)),
            new(new(-52, -239, -69)),
            new(new(-60, -236, -69)),
            new(new(-66, -231, -69)),
            new(new(-72, -226, -69)),
            new(new(-78, -221, -69)),
            new(new(-84, -215, -69)),
            new(new(-88, -208, -69)),
            new(new(-92, -201, -69)),
            new(new(-96, -193, -69)),
            new(new(-104, -189, -69)),
            new(new(-113, -188, -68)),
            new(new(-120, -185, -69)),
            new(new(-129, -184, -68)),
            new(new(-138, -186, -68)),
            new(new(-144, -193, -68)),
            new(new(-147, -200, -68)),
            new(new(-150, -207, -69)),
            new(new(-149, -216, -70)),
            new(new(-145, -223, -70)),
            new(new(-140, -229, -69)),
            new(new(-134, -235, -68)),
            new(new(-129, -241, -65)),
            new(new(-124, -247, -62)),
            new(new(-118, -254, -60)),
            new(new(-113, -260, -58)),
            new(new(-109, -268, -58)),
            new(new(-110, -277, -60)),
            new(new(-110, -286, -61)),
            new(new(-110, -294, -62)),
            new(new(-111, -303, -63)),
            new(new(-116, -309, -63)),
            new(new(-125, -311, -63)),
            new(new(-134, -311, -64)),
            new(new(-143, -310, -64)),
            new(new(-151, -310, -65)),
            new(new(-158, -314, -66)),
            new(new(-165, -318, -67)),
            new(new(-171, -324, -68)),
            new(new(-173, -333, -69)),
            new(new(-176, -341, -70)),
            new(new(-181, -348, -71)),
            new(new(-189, -346, -71)),
            new(new(-196, -343, -71)),
            new(new(-202, -338, -71)),
            new(new(-209, -332, -71)),
            new(new(-214, -325, -71)),
            new(new(-219, -317, -71)),
            new(new(-224, -311, -71)),
            new(new(-230, -305, -71)),
            new(new(-236, -300, -70)),
            new(new(-243, -297, -69)),
            new(new(-251, -295, -68)),
            new(new(-260, -295, -68)),
            new(new(-268, -300, -68)),
            new(new(-276, -304, -69)),
            new(new(-284, -308, -69)),
            new(new(-280, -315, -69)),
            new(new(-273, -309, -69)),
            new(new(-268, -302, -68)),
            new(new(-261, -296, -68)),
            new(new(-258, -289, -67)),
            new(new(-260, -280, -66)),
            new(new(-263, -272, -66)),
            new(new(-268, -265, -65)),
            new(new(-273, -259, -64)),
            new(new(-279, -254, -64)),
            new(new(-286, -250, -64)),
            new(new(-293, -245, -64)),
            new(new(-295, -236, -64)),
            new(new(-292, -229, -64)),
            new(new(-286, -223, -64)),
            new(new(-278, -218, -64)),
            new(new(-271, -215, -64)),
            new(new(-264, -212, -64)),
            new(new(-259, -205, -61)),
            new(new(-260, -196, -60)),
            new(new(-263, -189, -59)),
            new(new(-264, -181, -61)),
            new(new(-270, -176, -60)),
            new(new(-277, -172, -61)),
            new(new(-285, -167, -63)),
            new(new(-292, -162, -62)),
            new(new(-298, -157, -61)),
            new(new(-304, -152, -61)),
            new(new(-309, -146, -61)),
            new(new(-316, -141, -62)),
            new(new(-322, -136, -62)),
            new(new(-329, -131, -62)),
            new(new(-336, -126, -63)),
            new(new(-340, -118, -63)),
            new(new(-339, -110, -64)),
            new(new(-337, -102, -65)),
            new(new(-336, -94, -66)),
            new(new(-334, -85, -66)),
            new(new(-332, -76, -66)),
            new(new(-329, -69, -65)),
            new(new(-326, -62, -65)),
            new(new(-323, -55, -65)),
            new(new(-318, -49, -64)),
            new(new(-313, -42, -63)),
            new(new(-308, -35, -62)),
            new(new(-303, -28, -61)),
            new(new(-299, -21, -60)),
            new(new(-295, -13, -60)),
            new(new(-292, -6, -59)),
            new(new(-280, 11, -61)),
            new(new(-276, 19, -61)),
            new(new(-273, 26, -60)),
            new(new(-270, 35, -58)),
            new(new(-268, 43, -56)),
            new(new(-265, 50, -54)),
            new(new(-257, 55, -54)),
            new(new(-249, 57, -52)),
            new(new(-241, 59, -51)),
            new(new(-233, 59, -50)),
            new(new(-224, 59, -49)),
            new(new(-215, 59, -50)),
            new(new(-207, 59, -49)),
            new(new(-198, 60, -49)),
            new(new(-189, 63, -47)),
            new(new(-180, 62, -44)),
            new(new(-172, 57, -42)),
            new(new(-165, 52, -39)),
            new(new(-160, 46, -37)),
            new(new(-155, 40, -34)),
            new(new(-151, 34, -31)),
            new(new(-147, 27, -29)),
            new(new(-142, 21, -28)),
            new(new(-138, 14, -28)),
            new(new(-137, 5, -28)),
            new(new(-134, -3, -28)),
            new(new(-132, -11, -28)),
            new(new(-128, -18, -29)),
            new(new(-122, -24, -29)),
            new(new(-114, -23, -28)),
            new(new(-109, -16, -28)),
            new(new(-107, -7, -29)),
            new(new(-104, 0, -30)),
            new(new(-100, 7, -30)),
            new(new(-96, 15, -31)),
            new(new(-91, 22, -31)),
            new(new(-86, 28, -31)),
            new(new(-81, 34, -31)),
            new(new(-73, 39, -31)),
            new(new(-66, 42, -30)),
            new(new(-54, 45, -29), DungeonNodeType.Jump, "Jump"),
            new(new(-48, 45, -108)),
            new(new(-53, 51, -107)),
            new(new(-58, 57, -107)),
            new(new(-59, 65, -106)),
            new(new(-57, 73, -106)),
            new(new(-55, 82, -106)),
            new(new(-53, 91, -106)),
            new(new(-50, 99, -106)),
            new(new(-44, 104, -106)),
            new(new(-38, 109, -106)),
            new(new(-32, 116, -106)),
            new(new(-27, 124, -106)),
            new(new(-23, 131, -106)),
            new(new(-19, 138, -106)),
            new(new(-19, 146, -106)),
            new(new(-20, 155, -106)),
            new(new(-20, 163, -105)),
            new(new(-20, 172, -105)),
            new(new(-21, 181, -104)),
            new(new(-21, 189, -103)),
            new(new(-22, 196, -100)),
            new(new(-27, 202, -98)),
            new(new(-35, 205, -97)),
            new(new(-43, 203, -96)),
            new(new(-52, 206, -96)),
            new(new(-61, 207, -94)),
            new(new(-68, 210, -93)),
            new(new(-75, 215, -94)),
            new(new(-80, 221, -94)),
            new(new(-85, 227, -93)),
            new(new(-92, 230, -90)),
            new(new(-101, 230, -91)),
            new(new(-108, 225, -88)),
            new(new(-110, 217, -86)),
            new(new(-111, 208, -83)),
            new(new(-110, 199, -81)),
            new(new(-108, 190, -80)),
            new(new(-106, 181, -79)),
            new(new(-106, 173, -79)),
            new(new(-109, 166, -80)),
            new(new(-110, 157, -80)),
            new(new(-111, 148, -81)),
            new(new(-115, 141, -81)),
            new(new(-121, 135, -79)),
            new(new(-127, 130, -79)),
            // new(new(-132, 127, -79), DungeonNodeType.Talk, "1941"),
            // new(new(115, 243, -96), DungeonNodeType.Protect, "4216"),
        };

        public List<int> PriorityUnits { get; } = new();

        public int RequiredItemLevel { get; } = 10;

        public int RequiredLevel { get; } = 17;

        public Vector3 WorldEntry { get; } = new(-743, -2213, 16);

        public WowMapId WorldEntryMapId { get; } = WowMapId.Kalimdor;
    }
}