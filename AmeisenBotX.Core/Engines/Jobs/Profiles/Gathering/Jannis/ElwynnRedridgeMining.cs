﻿using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Jobs.Enums;
using AmeisenBotX.Wow.Objects.Enums;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Jobs.Profiles.Gathering.Jannis
{
    public class ElwynnRedridgeMining : IMiningProfile
    {
        public bool IsCirclePath { get; } = true;

        public JobType JobType { get; } = JobType.Mining;

        public List<Vector3> MailboxNodes { get; } = [new(-9456, 47, 57), new(-9250, -2145, 64)];

        public List<WowOreId> OreTypes { get; } = [WowOreId.Copper, WowOreId.Tin, WowOreId.Silver, WowOreId.Iron];

        public List<Vector3> Path { get; } =
        [
            new(-9289, -197, 69),
            new(-9295, -209, 69),
            new(-9301, -219, 68),
            new(-9309, -230, 68),
            new(-9317, -241, 68),
            new(-9320, -255, 69),
            new(-9325, -268, 69),
            new(-9333, -278, 69),
            new(-9341, -287, 68),
            new(-9349, -298, 67),
            new(-9350, -312, 68),
            new(-9350, -326, 67),
            new(-9350, -340, 65),
            new(-9349, -353, 66),
            new(-9347, -365, 68),
            new(-9344, -378, 67),
            new(-9340, -389, 66),
            new(-9333, -401, 66),
            new(-9324, -411, 67),
            new(-9318, -421, 69),
            new(-9314, -434, 68),
            new(-9314, -448, 68),
            new(-9316, -461, 69),
            new(-9322, -473, 72),
            new(-9330, -482, 72),
            new(-9340, -491, 69),
            new(-9350, -497, 70),
            new(-9360, -503, 70),
            new(-9366, -514, 69),
            new(-9370, -525, 69),
            new(-9377, -535, 69),
            new(-9385, -546, 69),
            new(-9389, -557, 70),
            new(-9395, -569, 68),
            new(-9401, -579, 67),
            new(-9407, -591, 68),
            new(-9410, -604, 68),
            new(-9406, -615, 72),
            new(-9396, -621, 69),
            new(-9386, -629, 69),
            new(-9377, -638, 68),
            new(-9367, -647, 68),
            new(-9356, -654, 68),
            new(-9346, -660, 68),
            new(-9335, -665, 69),
            new(-9322, -667, 68),
            new(-9311, -664, 66),
            new(-9299, -656, 66),
            new(-9287, -651, 65),
            new(-9273, -648, 65),
            new(-9260, -644, 64),
            new(-9248, -638, 63),
            new(-9238, -632, 62),
            new(-9227, -627, 62),
            new(-9215, -621, 61),
            new(-9203, -615, 61),
            new(-9192, -610, 61),
            new(-9180, -604, 63),
            new(-9167, -599, 62),
            new(-9156, -595, 59),
            new(-9143, -595, 58),
            new(-9130, -592, 58),
            new(-9122, -580, 59),
            new(-9116, -568, 59),
            new(-9105, -565, 61),
            new(-9092, -562, 62),
            new(-9085, -553, 60),
            new(-9074, -545, 59),
            new(-9061, -547, 58),
            new(-9050, -551, 56),
            new(-9037, -555, 55),
            new(-9027, -561, 55),
            new(-9024, -574, 55),
            new(-9029, -586, 57),
            new(-9035, -596, 56),
            new(-9043, -605, 53),
            new(-9053, -614, 53),
            new(-9041, -610, 53),
            new(-9035, -600, 56),
            new(-9030, -589, 56),
            new(-9027, -576, 56),
            new(-9030, -563, 55),
            new(-9040, -554, 56),
            new(-9054, -550, 57),
            new(-9067, -547, 58),
            new(-9080, -546, 60),
            new(-9089, -555, 61),
            new(-9095, -565, 62),
            new(-9108, -564, 61),
            new(-9119, -571, 59),
            new(-9125, -583, 59),
            new(-9135, -594, 58),
            new(-9148, -594, 58),
            new(-9161, -596, 60),
            new(-9172, -600, 63),
            new(-9184, -606, 63),
            new(-9194, -612, 61),
            new(-9200, -624, 61),
            new(-9197, -635, 63),
            new(-9193, -646, 65),
            new(-9197, -658, 64),
            new(-9208, -665, 63),
            new(-9222, -666, 63),
            new(-9232, -675, 63),
            new(-9243, -679, 63),
            new(-9256, -678, 64),
            new(-9269, -682, 65),
            new(-9280, -686, 64),
            new(-9290, -696, 64),
            new(-9295, -708, 63),
            new(-9297, -720, 63),
            new(-9298, -733, 66),
            new(-9298, -745, 67),
            new(-9297, -758, 68),
            new(-9296, -770, 66),
            new(-9290, -783, 65),
            new(-9281, -793, 66),
            new(-9270, -801, 66),
            new(-9258, -808, 66),
            new(-9247, -816, 66),
            new(-9241, -828, 68),
            new(-9241, -842, 69),
            new(-9238, -855, 69),
            new(-9227, -863, 69),
            new(-9216, -867, 68),
            new(-9203, -866, 69),
            new(-9190, -863, 70),
            new(-9178, -859, 70),
            new(-9165, -856, 70),
            new(-9151, -854, 69),
            new(-9138, -852, 68),
            new(-9125, -848, 69),
            new(-9114, -844, 69),
            new(-9103, -840, 71),
            new(-9091, -834, 71),
            new(-9080, -829, 70),
            new(-9068, -824, 70),
            new(-9056, -821, 69),
            new(-9043, -820, 69),
            new(-9034, -829, 68),
            new(-9030, -840, 69),
            new(-9024, -852, 70),
            new(-9014, -859, 69),
            new(-9002, -864, 70),
            new(-8990, -871, 69),
            new(-8984, -883, 67),
            new(-8973, -892, 67),
            new(-8960, -896, 66),
            new(-8948, -902, 68),
            new(-8940, -911, 69),
            new(-8927, -915, 72),
            new(-8914, -915, 73),
            new(-8900, -913, 74),
            new(-8887, -910, 75),
            new(-8875, -908, 76),
            new(-8862, -906, 76),
            new(-8850, -907, 76),
            new(-8837, -908, 76),
            new(-8826, -912, 76),
            new(-8817, -923, 76),
            new(-8808, -932, 74),
            new(-8801, -942, 73),
            new(-8796, -954, 73),
            new(-8792, -965, 73),
            new(-8790, -977, 73),
            new(-8791, -990, 75),
            new(-8792, -1002, 75),
            new(-8791, -1015, 74),
            new(-8789, -1027, 74),
            new(-8789, -1040, 76),
            new(-8790, -1052, 78),
            new(-8791, -1066, 78),
            new(-8796, -1078, 78),
            new(-8799, -1090, 77),
            new(-8801, -1103, 75),
            new(-8800, -1117, 76),
            new(-8798, -1131, 76),
            new(-8802, -1143, 77),
            new(-8814, -1150, 78),
            new(-8825, -1154, 78),
            new(-8838, -1157, 78),
            new(-8849, -1162, 78),
            new(-8862, -1160, 77),
            new(-8872, -1152, 76),
            new(-8880, -1143, 77),
            new(-8890, -1134, 75),
            new(-8901, -1126, 73),
            new(-8912, -1121, 71),
            new(-8923, -1118, 69),
            new(-8936, -1119, 68),
            new(-8947, -1124, 66),
            new(-8954, -1136, 65),
            new(-8950, -1147, 67),
            new(-8941, -1158, 70),
            new(-8939, -1171, 71),
            new(-8943, -1184, 73),
            new(-8949, -1197, 73),
            new(-8955, -1209, 74),
            new(-8961, -1219, 74),
            new(-8967, -1229, 76),
            new(-8974, -1240, 77),
            new(-8982, -1249, 76),
            new(-8994, -1254, 76),
            new(-9006, -1254, 76),
            new(-9020, -1253, 76),
            new(-9033, -1253, 75),
            new(-9045, -1254, 74),
            new(-9058, -1254, 72),
            new(-9070, -1255, 73),
            new(-9083, -1258, 74),
            new(-9094, -1262, 75),
            new(-9107, -1263, 75),
            new(-9116, -1253, 73),
            new(-9123, -1241, 72),
            new(-9135, -1236, 73),
            new(-9148, -1239, 73),
            new(-9162, -1238, 73),
            new(-9175, -1239, 74),
            new(-9187, -1241, 74),
            new(-9200, -1243, 76),
            new(-9212, -1244, 76),
            new(-9224, -1249, 77),
            new(-9232, -1258, 77),
            new(-9241, -1268, 75),
            new(-9249, -1277, 74),
            new(-9258, -1287, 73),
            new(-9267, -1297, 74),
            new(-9277, -1307, 73),
            new(-9286, -1317, 72),
            new(-9292, -1327, 70),
            new(-9297, -1340, 70),
            new(-9305, -1351, 70),
            new(-9311, -1361, 70),
            new(-9319, -1372, 69),
            new(-9325, -1382, 67),
            new(-9331, -1394, 65),
            new(-9337, -1407, 64),
            new(-9341, -1418, 64),
            new(-9343, -1431, 66),
            new(-9346, -1444, 67),
            new(-9351, -1455, 67),
            new(-9357, -1465, 68),
            new(-9365, -1476, 67),
            new(-9374, -1486, 67),
            new(-9384, -1496, 68),
            new(-9395, -1504, 69),
            new(-9407, -1510, 69),
            new(-9418, -1514, 69),
            new(-9432, -1516, 69),
            new(-9445, -1517, 67),
            new(-9457, -1517, 65),
            new(-9470, -1518, 64),
            new(-9482, -1521, 63),
            new(-9495, -1525, 63),
            new(-9508, -1529, 61),
            new(-9519, -1533, 60),
            new(-9529, -1539, 61),
            new(-9539, -1548, 62),
            new(-9550, -1556, 61),
            new(-9560, -1563, 61),
            new(-9571, -1571, 62),
            new(-9582, -1578, 62),
            new(-9592, -1585, 61),
            new(-9603, -1593, 60),
            new(-9612, -1603, 60),
            new(-9620, -1615, 58),
            new(-9621, -1628, 57),
            new(-9619, -1642, 57),
            new(-9616, -1655, 57),
            new(-9613, -1669, 56),
            new(-9611, -1682, 56),
            new(-9608, -1696, 56),
            new(-9607, -1709, 56),
            new(-9607, -1721, 56),
            new(-9608, -1734, 57),
            new(-9610, -1746, 57),
            new(-9611, -1760, 55),
            new(-9613, -1773, 53),
            new(-9621, -1782, 52),
            new(-9634, -1786, 53),
            new(-9647, -1791, 54),
            new(-9659, -1796, 56),
            new(-9669, -1802, 57),
            new(-9678, -1809, 59),
            new(-9687, -1819, 58),
            new(-9686, -1832, 60),
            new(-9676, -1841, 58),
            new(-9665, -1850, 59),
            new(-9659, -1862, 59),
            new(-9653, -1872, 59),
            new(-9647, -1882, 58),
            new(-9639, -1892, 58),
            new(-9628, -1896, 59),
            new(-9614, -1896, 59),
            new(-9601, -1896, 60),
            new(-9589, -1899, 61),
            new(-9576, -1901, 64),
            new(-9575, -1915, 64),
            new(-9572, -1928, 66),
            new(-9569, -1939, 68),
            new(-9564, -1952, 69),
            new(-9558, -1962, 70),
            new(-9549, -1972, 71),
            new(-9540, -1982, 72),
            new(-9531, -1991, 74),
            new(-9517, -1989, 77),
            new(-9507, -1984, 81),
            new(-9498, -1977, 89),
            new(-9494, -1987, 94),
            new(-9499, -1999, 90),
            new(-9509, -1997, 81),
            new(-9521, -1998, 77),
            new(-9529, -2009, 76),
            new(-9535, -2019, 74),
            new(-9543, -2030, 71),
            new(-9549, -2040, 70),
            new(-9556, -2051, 69),
            new(-9564, -2060, 68),
            new(-9561, -2072, 68),
            new(-9553, -2081, 72),
            new(-9550, -2092, 79),
            new(-9549, -2105, 81),
            new(-9545, -2116, 85),
            new(-9541, -2127, 88),
            new(-9541, -2140, 91),
            new(-9543, -2154, 92),
            new(-9540, -2165, 95),
            new(-9543, -2178, 95),
            new(-9553, -2184, 92),
            new(-9564, -2185, 88),
            new(-9576, -2187, 89),
            new(-9585, -2196, 90),
            new(-9583, -2209, 93),
            new(-9577, -2219, 95),
            new(-9569, -2227, 91),
            new(-9563, -2237, 87),
            new(-9560, -2248, 85),
            new(-9554, -2258, 85),
            new(-9544, -2266, 83),
            new(-9532, -2267, 81),
            new(-9521, -2268, 77),
            new(-9510, -2272, 77),
            new(-9499, -2276, 76),
            new(-9486, -2280, 75),
            new(-9474, -2280, 75),
            new(-9461, -2279, 74),
            new(-9450, -2276, 72),
            new(-9436, -2273, 70),
            new(-9423, -2271, 69),
            new(-9411, -2271, 67),
            new(-9397, -2271, 68),
            new(-9384, -2272, 70),
            new(-9372, -2274, 71),
            new(-9359, -2277, 72),
            new(-9347, -2276, 72),
            new(-9334, -2276, 72),
            new(-9322, -2277, 71),
            new(-9308, -2278, 70),
            new(-9295, -2278, 68),
            new(-9283, -2277, 68),
            new(-9276, -2266, 67),
            new(-9275, -2253, 65),
            new(-9274, -2240, 64),
            new(-9272, -2228, 64),
            new(-9270, -2215, 64),
            new(-9267, -2203, 64),
            new(-9263, -2190, 64),
            new(-9259, -2179, 64),
            new(-9256, -2166, 64),
            new(-9253, -2153, 64),
            new(-9249, -2140, 64),
            new(-9248, -2126, 64),
            new(-9247, -2113, 67),
            new(-9244, -2102, 71),
            new(-9245, -2090, 75),
            new(-9243, -2078, 75),
            new(-9242, -2066, 76),
            new(-9246, -2053, 77),
            new(-9248, -2039, 77),
            new(-9250, -2026, 77),
            new(-9252, -2014, 77),
            new(-9254, -2001, 77),
            new(-9256, -1987, 78),
            new(-9251, -1977, 81),
            new(-9245, -1964, 82),
            new(-9240, -1952, 85),
            new(-9231, -1945, 91),
            new(-9222, -1943, 98),
            new(-9211, -1946, 105),
            new(-9204, -1954, 111),
            new(-9198, -1964, 113),
            new(-9192, -1976, 113),
            new(-9185, -1988, 114),
            new(-9179, -1998, 114),
            new(-9172, -2008, 115),
            new(-9165, -2019, 117),
            new(-9159, -2031, 119),
            new(-9152, -2041, 122),
            new(-9145, -2050, 125),
            new(-9140, -2062, 127),
            new(-9140, -2074, 124),
            new(-9138, -2087, 122),
            new(-9136, -2101, 121),
            new(-9134, -2115, 122),
            new(-9132, -2128, 123),
            new(-9134, -2141, 122),
            new(-9139, -2153, 121),
            new(-9137, -2167, 121),
            new(-9136, -2180, 121),
            new(-9136, -2193, 121),
            new(-9140, -2204, 121),
            new(-9144, -2215, 120),
            new(-9148, -2226, 119),
            new(-9152, -2237, 118),
            new(-9156, -2250, 116),
            new(-9158, -2261, 113),
            new(-9158, -2275, 113),
            new(-9155, -2288, 114),
            new(-9150, -2299, 115),
            new(-9145, -2311, 116),
            new(-9142, -2323, 115),
            new(-9139, -2336, 115),
            new(-9135, -2349, 116),
            new(-9129, -2361, 117),
            new(-9121, -2370, 119),
            new(-9113, -2380, 121),
            new(-9107, -2390, 122),
            new(-9103, -2401, 121),
            new(-9094, -2412, 122),
            new(-9083, -2418, 127),
            new(-9075, -2428, 129),
            new(-9075, -2442, 126),
            new(-9076, -2455, 126),
            new(-9080, -2466, 123),
            new(-9085, -2477, 120),
            new(-9088, -2488, 118),
            new(-9090, -2501, 119),
            new(-9092, -2513, 118),
            new(-9087, -2525, 119),
            new(-9078, -2535, 121),
            new(-9065, -2541, 124),
            new(-9052, -2544, 126),
            new(-9042, -2553, 127),
            new(-9037, -2564, 126),
            new(-9028, -2574, 127),
            new(-9019, -2584, 127),
            new(-9010, -2594, 128),
            new(-9011, -2607, 128),
            new(-9018, -2618, 128),
            new(-9030, -2624, 127),
            new(-9036, -2634, 128),
            new(-9043, -2643, 130),
            new(-9054, -2651, 130),
            new(-9064, -2657, 128),
            new(-9075, -2660, 124),
            new(-9086, -2654, 121),
            new(-9095, -2644, 120),
            new(-9105, -2638, 120),
            new(-9113, -2629, 118),
            new(-9118, -2617, 115),
            new(-9121, -2603, 114),
            new(-9123, -2590, 114),
            new(-9129, -2580, 115),
            new(-9140, -2572, 116),
            new(-9148, -2561, 117),
            new(-9152, -2550, 116),
            new(-9154, -2539, 113),
            new(-9154, -2525, 112),
            new(-9153, -2514, 117),
            new(-9149, -2501, 118),
            new(-9145, -2490, 117),
            new(-9146, -2476, 116),
            new(-9151, -2464, 114),
            new(-9154, -2452, 111),
            new(-9154, -2439, 107),
            new(-9147, -2427, 107),
            new(-9147, -2413, 106),
            new(-9149, -2400, 103),
            new(-9154, -2390, 100),
            new(-9163, -2382, 97),
            new(-9174, -2375, 93),
            new(-9185, -2369, 91),
            new(-9197, -2364, 89),
            new(-9206, -2357, 87),
            new(-9215, -2349, 86),
            new(-9224, -2339, 84),
            new(-9232, -2330, 83),
            new(-9241, -2320, 80),
            new(-9250, -2312, 77),
            new(-9260, -2304, 73),
            new(-9270, -2298, 69),
            new(-9281, -2294, 68),
            new(-9294, -2292, 68),
            new(-9306, -2291, 70),
            new(-9320, -2290, 71),
            new(-9334, -2288, 72),
            new(-9347, -2287, 72),
            new(-9359, -2286, 72),
            new(-9372, -2285, 71),
            new(-9384, -2284, 70),
            new(-9397, -2283, 69),
            new(-9409, -2283, 68),
            new(-9423, -2283, 69),
            new(-9436, -2283, 71),
            new(-9448, -2282, 72),
            new(-9461, -2280, 74),
            new(-9473, -2279, 74),
            new(-9487, -2279, 76),
            new(-9500, -2280, 76),
            new(-9514, -2284, 75),
            new(-9526, -2290, 74),
            new(-9538, -2296, 72),
            new(-9549, -2301, 71),
            new(-9560, -2305, 69),
            new(-9573, -2308, 68),
            new(-9586, -2310, 66),
            new(-9598, -2312, 67),
            new(-9609, -2314, 70),
            new(-9620, -2318, 72),
            new(-9632, -2324, 72),
            new(-9642, -2330, 71),
            new(-9653, -2334, 70),
            new(-9667, -2336, 68),
            new(-9681, -2336, 66),
            new(-9692, -2334, 62),
            new(-9703, -2332, 66),
            new(-9714, -2327, 68),
            new(-9723, -2318, 70),
            new(-9728, -2307, 70),
            new(-9728, -2294, 66),
            new(-9725, -2283, 64),
            new(-9721, -2270, 63),
            new(-9718, -2257, 61),
            new(-9714, -2244, 60),
            new(-9712, -2232, 59),
            new(-9708, -2219, 58),
            new(-9706, -2205, 59),
            new(-9702, -2194, 58),
            new(-9700, -2182, 58),
            new(-9703, -2169, 59),
            new(-9703, -2157, 59),
            new(-9703, -2144, 60),
            new(-9703, -2132, 60),
            new(-9700, -2119, 59),
            new(-9695, -2108, 59),
            new(-9686, -2098, 59),
            new(-9674, -2092, 58),
            new(-9663, -2088, 58),
            new(-9650, -2085, 59),
            new(-9638, -2084, 61),
            new(-9625, -2088, 61),
            new(-9613, -2083, 63),
            new(-9605, -2074, 63),
            new(-9595, -2065, 64),
            new(-9587, -2054, 65),
            new(-9585, -2040, 66),
            new(-9585, -2027, 66),
            new(-9585, -2015, 66),
            new(-9588, -2001, 66),
            new(-9586, -1988, 66),
            new(-9583, -1975, 67),
            new(-9581, -1961, 67),
            new(-9584, -1948, 66),
            new(-9591, -1938, 64),
            new(-9600, -1928, 63),
            new(-9610, -1922, 62),
            new(-9622, -1916, 60),
            new(-9633, -1912, 59),
            new(-9644, -1907, 57),
            new(-9656, -1903, 56),
            new(-9667, -1899, 55),
            new(-9680, -1895, 54),
            new(-9691, -1891, 52),
            new(-9702, -1887, 51),
            new(-9715, -1884, 50),
            new(-9727, -1883, 50),
            new(-9741, -1882, 50),
            new(-9754, -1881, 49),
            new(-9768, -1878, 47),
            new(-9778, -1870, 46),
            new(-9787, -1861, 45),
            new(-9792, -1849, 43),
            new(-9796, -1838, 43),
            new(-9804, -1830, 36),
            new(-9811, -1821, 29),
            new(-9815, -1810, 26),
            new(-9821, -1798, 24),
            new(-9826, -1787, 23),
            new(-9832, -1777, 23),
            new(-9839, -1766, 23),
            new(-9846, -1756, 23),
            new(-9852, -1744, 23),
            new(-9855, -1731, 24),
            new(-9854, -1719, 24),
            new(-9853, -1706, 24),
            new(-9852, -1694, 24),
            new(-9851, -1681, 23),
            new(-9850, -1669, 23),
            new(-9849, -1655, 22),
            new(-9846, -1642, 23),
            new(-9842, -1628, 25),
            new(-9836, -1616, 27),
            new(-9831, -1606, 30),
            new(-9825, -1596, 31),
            new(-9819, -1584, 31),
            new(-9813, -1574, 33),
            new(-9808, -1564, 36),
            new(-9804, -1553, 39),
            new(-9800, -1542, 41),
            new(-9796, -1529, 42),
            new(-9792, -1516, 43),
            new(-9790, -1504, 43),
            new(-9791, -1491, 44),
            new(-9794, -1479, 44),
            new(-9799, -1467, 43),
            new(-9805, -1457, 41),
            new(-9812, -1448, 39),
            new(-9821, -1438, 39),
            new(-9829, -1429, 39),
            new(-9839, -1419, 38),
            new(-9849, -1410, 39),
            new(-9857, -1399, 38),
            new(-9862, -1387, 36),
            new(-9863, -1375, 36),
            new(-9863, -1361, 37),
            new(-9861, -1348, 39),
            new(-9859, -1336, 39),
            new(-9856, -1323, 38),
            new(-9852, -1310, 37),
            new(-9849, -1299, 35),
            new(-9845, -1288, 36),
            new(-9841, -1275, 37),
            new(-9839, -1263, 36),
            new(-9836, -1250, 36),
            new(-9835, -1236, 36),
            new(-9836, -1223, 36),
            new(-9841, -1212, 34),
            new(-9847, -1200, 33),
            new(-9847, -1188, 33),
            new(-9843, -1175, 32),
            new(-9834, -1164, 32),
            new(-9823, -1158, 32),
            new(-9812, -1154, 33),
            new(-9801, -1145, 33),
            new(-9792, -1135, 34),
            new(-9782, -1126, 36),
            new(-9776, -1116, 36),
            new(-9775, -1104, 36),
            new(-9779, -1091, 35),
            new(-9782, -1080, 32),
            new(-9784, -1068, 34),
            new(-9787, -1057, 36),
            new(-9790, -1046, 38),
            new(-9794, -1035, 39),
            new(-9800, -1023, 38),
            new(-9806, -1013, 38),
            new(-9816, -1003, 39),
            new(-9826, -993, 39),
            new(-9836, -984, 38),
            new(-9843, -973, 38),
            new(-9846, -961, 38),
            new(-9847, -948, 38),
            new(-9846, -936, 39),
            new(-9843, -925, 41),
            new(-9839, -914, 42),
            new(-9840, -901, 41),
            new(-9842, -889, 39),
            new(-9844, -876, 40),
            new(-9840, -863, 40),
            new(-9831, -855, 40),
            new(-9818, -854, 39),
            new(-9806, -852, 39),
            new(-9794, -846, 39),
            new(-9781, -840, 40),
            new(-9768, -836, 40),
            new(-9755, -832, 39),
            new(-9741, -830, 40),
            new(-9728, -829, 43),
            new(-9714, -830, 43),
            new(-9701, -832, 45),
            new(-9689, -830, 45),
            new(-9678, -826, 46),
            new(-9667, -818, 48),
            new(-9661, -806, 46),
            new(-9656, -795, 45),
            new(-9650, -783, 44),
            new(-9646, -772, 44),
            new(-9639, -761, 45),
            new(-9629, -755, 45),
            new(-9617, -757, 43),
            new(-9607, -764, 40),
            new(-9595, -771, 41),
            new(-9587, -781, 42),
            new(-9591, -793, 42),
            new(-9599, -802, 43),
            new(-9605, -812, 45),
            new(-9611, -824, 44),
            new(-9621, -830, 44),
            new(-9634, -826, 44),
            new(-9645, -822, 44),
            new(-9656, -818, 45),
            new(-9667, -814, 48),
            new(-9679, -808, 47),
            new(-9688, -801, 45),
            new(-9699, -793, 44),
            new(-9708, -785, 43),
            new(-9718, -776, 43),
            new(-9726, -765, 42),
            new(-9732, -755, 42),
            new(-9736, -744, 42),
            new(-9739, -731, 41),
            new(-9743, -720, 41),
            new(-9747, -707, 42),
            new(-9750, -694, 40),
            new(-9751, -682, 40),
            new(-9752, -668, 42),
            new(-9754, -655, 41),
            new(-9757, -643, 41),
            new(-9761, -630, 40),
            new(-9764, -617, 39),
            new(-9767, -604, 38),
            new(-9768, -592, 37),
            new(-9767, -578, 37),
            new(-9765, -565, 38),
            new(-9764, -553, 36),
            new(-9768, -542, 35),
            new(-9777, -533, 32),
            new(-9783, -523, 31),
            new(-9786, -509, 31),
            new(-9789, -496, 33),
            new(-9791, -482, 34),
            new(-9783, -473, 36),
            new(-9773, -467, 38),
            new(-9769, -456, 39),
            new(-9764, -444, 41),
            new(-9760, -433, 42),
            new(-9757, -422, 45),
            new(-9756, -411, 50),
            new(-9759, -400, 55),
            new(-9767, -391, 57),
            new(-9778, -388, 55),
            new(-9788, -379, 54),
            new(-9792, -366, 55),
            new(-9795, -355, 53),
            new(-9800, -344, 52),
            new(-9806, -332, 50),
            new(-9816, -322, 48),
            new(-9820, -309, 43),
            new(-9825, -297, 40),
            new(-9834, -289, 41),
            new(-9842, -280, 40),
            new(-9851, -270, 38),
            new(-9860, -260, 36),
            new(-9866, -250, 35),
            new(-9874, -239, 35),
            new(-9879, -228, 35),
            new(-9874, -216, 36),
            new(-9879, -204, 36),
            new(-9889, -198, 35),
            new(-9901, -193, 33),
            new(-9911, -186, 33),
            new(-9915, -173, 32),
            new(-9921, -161, 31),
            new(-9929, -153, 27),
            new(-9936, -144, 25),
            new(-9939, -131, 25),
            new(-9944, -120, 25),
            new(-9947, -109, 28),
            new(-9947, -95, 30),
            new(-9947, -82, 32),
            new(-9946, -70, 33),
            new(-9944, -57, 32),
            new(-9941, -45, 32),
            new(-9937, -32, 33),
            new(-9935, -19, 33),
            new(-9934, -7, 33),
            new(-9933, 7, 32),
            new(-9932, 20, 33),
            new(-9929, 32, 33),
            new(-9922, 43, 32),
            new(-9912, 53, 32),
            new(-9903, 63, 32),
            new(-9893, 72, 31),
            new(-9883, 82, 31),
            new(-9874, 92, 32),
            new(-9868, 102, 32),
            new(-9864, 113, 32),
            new(-9862, 126, 32),
            new(-9861, 138, 32),
            new(-9868, 149, 30),
            new(-9874, 159, 28),
            new(-9867, 168, 20),
            new(-9857, 177, 20),
            new(-9845, 184, 21),
            new(-9832, 183, 22),
            new(-9819, 181, 23),
            new(-9807, 179, 23),
            new(-9799, 168, 24),
            new(-9795, 155, 25),
            new(-9783, 150, 26),
            new(-9770, 154, 25),
            new(-9758, 149, 22),
            new(-9751, 139, 20),
            new(-9751, 127, 17),
            new(-9752, 114, 15),
            new(-9747, 102, 13),
            new(-9756, 93, 13),
            new(-9769, 94, 11),
            new(-9779, 99, 6),
            new(-9788, 109, 5),
            new(-9798, 115, 6),
            new(-9809, 118, 4),
            new(-9822, 124, 4),
            new(-9835, 128, 6),
            new(-9845, 134, 5),
            new(-9853, 144, 8),
            new(-9847, 154, 6),
            new(-9836, 162, 5),
            new(-9827, 172, 8),
            new(-9823, 185, 12),
            new(-9825, 198, 14),
            new(-9829, 211, 14),
            new(-9839, 217, 14),
            new(-9852, 218, 14),
            new(-9864, 219, 14),
            new(-9876, 219, 14),
            new(-9889, 220, 14),
            new(-9901, 221, 15),
            new(-9914, 223, 18),
            new(-9928, 226, 21),
            new(-9940, 232, 24),
            new(-9949, 242, 26),
            new(-9955, 252, 29),
            new(-9958, 263, 34),
            new(-9963, 275, 36),
            new(-9972, 286, 37),
            new(-9981, 297, 35),
            new(-9980, 310, 35),
            new(-9976, 321, 35),
            new(-9970, 333, 36),
            new(-9965, 344, 36),
            new(-9958, 355, 37),
            new(-9955, 366, 35),
            new(-9964, 377, 36),
            new(-9974, 386, 37),
            new(-9983, 397, 37),
            new(-9984, 409, 37),
            new(-9981, 422, 37),
            new(-9982, 434, 37),
            new(-9984, 448, 36),
            new(-9983, 461, 35),
            new(-9978, 472, 34),
            new(-9969, 481, 34),
            new(-9959, 488, 32),
            new(-9947, 494, 32),
            new(-9936, 499, 32),
            new(-9924, 504, 32),
            new(-9910, 508, 32),
            new(-9897, 511, 32),
            new(-9883, 513, 33),
            new(-9870, 512, 32),
            new(-9858, 511, 32),
            new(-9845, 510, 31),
            new(-9833, 509, 31),
            new(-9820, 506, 33),
            new(-9811, 498, 35),
            new(-9803, 488, 36),
            new(-9794, 477, 36),
            new(-9784, 468, 36),
            new(-9772, 467, 36),
            new(-9758, 468, 34),
            new(-9745, 471, 34),
            new(-9734, 475, 36),
            new(-9722, 481, 34),
            new(-9711, 484, 32),
            new(-9700, 488, 34),
            new(-9689, 492, 35),
            new(-9678, 496, 38),
            new(-9665, 496, 39),
            new(-9653, 495, 39),
            new(-9640, 495, 40),
            new(-9628, 496, 41),
            new(-9615, 498, 42),
            new(-9601, 501, 44),
            new(-9591, 495, 42),
            new(-9585, 485, 42),
            new(-9574, 487, 47),
            new(-9566, 496, 48),
            new(-9562, 509, 48),
            new(-9558, 522, 49),
            new(-9551, 534, 51),
            new(-9540, 541, 50),
            new(-9529, 545, 51),
            new(-9518, 549, 51),
            new(-9504, 552, 52),
            new(-9491, 554, 52),
            new(-9479, 549, 54),
            new(-9470, 541, 54),
            new(-9459, 533, 55),
            new(-9448, 525, 56),
            new(-9439, 517, 57),
            new(-9429, 508, 56),
            new(-9422, 499, 53),
            new(-9413, 489, 51),
            new(-9405, 481, 48),
            new(-9395, 474, 47),
            new(-9386, 466, 47),
            new(-9376, 459, 47),
            new(-9366, 451, 47),
            new(-9356, 446, 51),
            new(-9347, 435, 52),
            new(-9345, 422, 52),
            new(-9348, 409, 52),
            new(-9352, 396, 53),
            new(-9355, 383, 53),
            new(-9359, 372, 55),
            new(-9366, 362, 56),
            new(-9375, 353, 55),
            new(-9385, 347, 55),
            new(-9396, 342, 55),
            new(-9408, 336, 56),
            new(-9419, 328, 56),
            new(-9430, 319, 56),
            new(-9440, 310, 57),
            new(-9450, 301, 55),
            new(-9459, 293, 54),
            new(-9469, 284, 53),
            new(-9479, 274, 53),
            new(-9486, 263, 53),
            new(-9491, 252, 54),
            new(-9493, 239, 55),
            new(-9489, 225, 54),
            new(-9484, 215, 57),
            new(-9476, 204, 57),
            new(-9467, 194, 57),
            new(-9456, 187, 58),
            new(-9442, 185, 58),
            new(-9429, 183, 57),
            new(-9417, 182, 57),
            new(-9404, 178, 59),
            new(-9393, 174, 60),
            new(-9383, 168, 62),
            new(-9374, 159, 62),
            new(-9370, 148, 61),
            new(-9366, 137, 62),
            new(-9361, 124, 63),
            new(-9355, 114, 62),
            new(-9347, 103, 63),
            new(-9335, 99, 63),
            new(-9323, 99, 62),
            new(-9309, 100, 63),
            new(-9296, 100, 66),
            new(-9284, 99, 68),
            new(-9272, 93, 69),
            new(-9263, 86, 72),
            new(-9255, 77, 74),
            new(-9250, 65, 75),
            new(-9248, 52, 74),
            new(-9244, 39, 73),
            new(-9238, 29, 74),
            new(-9234, 18, 75),
            new(-9233, 6, 75),
            new(-9234, -7, 74),
            new(-9237, -21, 73),
            new(-9242, -33, 71),
            new(-9247, -44, 72),
            new(-9254, -55, 73),
            new(-9260, -65, 71),
            new(-9267, -75, 69),
            new(-9272, -87, 70),
            new(-9274, -99, 71),
            new(-9271, -112, 72),
            new(-9264, -124, 71),
            new(-9257, -136, 69),
            new(-9254, -147, 65),
            new(-9254, -160, 67),
            new(-9258, -171, 70),
            new(-9267, -181, 70),
            new(-9278, -189, 69),
        ];

        public override string ToString()
        {
            return "[20+][Mining] Elwynn Forest - Redridge Mountains (Jannis)";
        }
    }
}