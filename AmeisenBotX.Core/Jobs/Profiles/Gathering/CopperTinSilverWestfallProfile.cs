﻿using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Jobs.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Jobs.Profiles.Gathering
{
    public class CopperTinSilverWestfallProfile : IMiningProfile
    {
        public bool IsCirclePath => true;

        public JobType JobType => JobType.Mining;

        public List<Vector3> MailboxNodes { get; private set; } = new List<Vector3>()
        {
           new Vector3(-10643, 1157, 33),
        };

        public List<WowOreId> OreTypes { get; } = new List<WowOreId>()
        {
            WowOreId.Copper,
            WowOreId.Silver,
            WowOreId.Tin
        };

        public List<Vector3> Path { get; } = new List<Vector3>()
        {
            new Vector3(-9799, 872, 25),
            new Vector3(-9791, 878, 24),
            new Vector3(-9783, 884, 23),
            new Vector3(-9774, 888, 23),
            new Vector3(-9764, 891, 23),
            new Vector3(-9754, 895, 23),
            new Vector3(-9746, 901, 24),
            new Vector3(-9738, 907, 25),
            new Vector3(-9728, 909, 25),
            new Vector3(-9718, 911, 25),
            new Vector3(-9709, 914, 26),
            new Vector3(-9700, 915, 29),
            new Vector3(-9691, 917, 34),
            new Vector3(-9682, 920, 35),
            new Vector3(-9672, 923, 33),
            new Vector3(-9662, 927, 33),
            new Vector3(-9654, 932, 37),
            new Vector3(-9646, 937, 39),
            new Vector3(-9638, 943, 38),
            new Vector3(-9630, 950, 37),
            new Vector3(-9621, 954, 38),
            new Vector3(-9612, 959, 38),
            new Vector3(-9609, 968, 40),
            new Vector3(-9615, 976, 36),
            new Vector3(-9625, 977, 36),
            new Vector3(-9635, 980, 34),
            new Vector3(-9645, 983, 34),
            new Vector3(-9655, 985, 34),
            new Vector3(-9665, 989, 33),
            new Vector3(-9671, 997, 32),
            new Vector3(-9677, 1006, 32),
            new Vector3(-9683, 1014, 34),
            new Vector3(-9692, 1018, 36),
            new Vector3(-9702, 1016, 37),
            new Vector3(-9712, 1014, 38),
            new Vector3(-9722, 1013, 38),
            new Vector3(-9732, 1013, 37),
            new Vector3(-9742, 1014, 38),
            new Vector3(-9752, 1013, 38),
            new Vector3(-9758, 1021, 36),
            new Vector3(-9758, 1031, 35),
            new Vector3(-9758, 1041, 31),
            new Vector3(-9757, 1050, 27),
            new Vector3(-9756, 1059, 24),
            new Vector3(-9749, 1066, 23),
            new Vector3(-9742, 1074, 20),
            new Vector3(-9740, 1082, 15),
            new Vector3(-9747, 1089, 15),
            new Vector3(-9757, 1092, 17),
            new Vector3(-9766, 1096, 18),
            new Vector3(-9775, 1100, 21),
            new Vector3(-9784, 1105, 24),
            new Vector3(-9793, 1109, 27),
            new Vector3(-9802, 1113, 30),
            new Vector3(-9811, 1117, 34),
            new Vector3(-9820, 1122, 35),
            new Vector3(-9829, 1126, 36),
            new Vector3(-9838, 1130, 35),
            new Vector3(-9848, 1130, 34),
            new Vector3(-9858, 1128, 33),
            new Vector3(-9868, 1126, 33),
            new Vector3(-9878, 1124, 34),
            new Vector3(-9888, 1122, 34),
            new Vector3(-9898, 1120, 35),
            new Vector3(-9908, 1119, 36),
            new Vector3(-9913, 1128, 37),
            new Vector3(-9911, 1138, 40),
            new Vector3(-9908, 1147, 41),
            new Vector3(-9904, 1156, 42),
            new Vector3(-9905, 1166, 41),
            new Vector3(-9907, 1176, 41),
            new Vector3(-9913, 1184, 42),
            new Vector3(-9918, 1193, 42),
            new Vector3(-9921, 1203, 42),
            new Vector3(-9923, 1213, 42),
            new Vector3(-9925, 1223, 42),
            new Vector3(-9927, 1233, 42),
            new Vector3(-9930, 1243, 42),
            new Vector3(-9934, 1252, 42),
            new Vector3(-9938, 1261, 42),
            new Vector3(-9941, 1271, 42),
            new Vector3(-9946, 1280, 41),
            new Vector3(-9951, 1288, 43),
            new Vector3(-9957, 1296, 44),
            new Vector3(-9964, 1303, 44),
            new Vector3(-9972, 1309, 44),
            new Vector3(-9979, 1316, 43),
            new Vector3(-9986, 1323, 43),
            new Vector3(-9992, 1331, 42),
            new Vector3(-9998, 1339, 43),
            new Vector3(-10004, 1348, 44),
            new Vector3(-10009, 1357, 45),
            new Vector3(-10010, 1367, 45),
            new Vector3(-10010, 1377, 47),
            new Vector3(-10009, 1386, 44),
            new Vector3(-10008, 1395, 41),
            new Vector3(-10007, 1405, 41),
            new Vector3(-10007, 1415, 41),
            new Vector3(-10007, 1425, 41),
            new Vector3(-10005, 1435, 41),
            new Vector3(-10004, 1445, 41),
            new Vector3(-10001, 1455, 41),
            new Vector3(-9991, 1457, 42),
            new Vector3(-9981, 1456, 44),
            new Vector3(-9971, 1455, 45),
            new Vector3(-9961, 1455, 44),
            new Vector3(-9954, 1462, 42),
            new Vector3(-9949, 1470, 40),
            new Vector3(-9953, 1461, 42),
            new Vector3(-9948, 1453, 40),
            new Vector3(-9941, 1446, 40),
            new Vector3(-9934, 1439, 39),
            new Vector3(-9925, 1443, 39),
            new Vector3(-9915, 1443, 40),
            new Vector3(-9911, 1434, 39),
            new Vector3(-9907, 1425, 39),
            new Vector3(-9897, 1425, 40),
            new Vector3(-9887, 1425, 40),
            new Vector3(-9893, 1433, 40),
            new Vector3(-9900, 1426, 39),
            new Vector3(-9910, 1430, 39),
            new Vector3(-9911, 1440, 39),
            new Vector3(-9912, 1450, 41),
            new Vector3(-9904, 1456, 41),
            new Vector3(-9894, 1456, 42),
            new Vector3(-9885, 1451, 43),
            new Vector3(-9881, 1442, 44),
            new Vector3(-9879, 1432, 44),
            new Vector3(-9877, 1422, 44),
            new Vector3(-9880, 1432, 44),
            new Vector3(-9880, 1442, 44),
            new Vector3(-9874, 1450, 43),
            new Vector3(-9866, 1456, 42),
            new Vector3(-9856, 1457, 41),
            new Vector3(-9848, 1450, 40),
            new Vector3(-9840, 1443, 39),
            new Vector3(-9833, 1435, 37),
            new Vector3(-9827, 1427, 37),
            new Vector3(-9822, 1418, 36),
            new Vector3(-9827, 1409, 37),
            new Vector3(-9836, 1405, 38),
            new Vector3(-9846, 1404, 38),
            new Vector3(-9853, 1397, 38),
            new Vector3(-9854, 1387, 38),
            new Vector3(-9846, 1393, 38),
            new Vector3(-9838, 1400, 38),
            new Vector3(-9829, 1406, 37),
            new Vector3(-9822, 1413, 37),
            new Vector3(-9824, 1423, 37),
            new Vector3(-9830, 1431, 37),
            new Vector3(-9836, 1439, 38),
            new Vector3(-9843, 1446, 39),
            new Vector3(-9850, 1453, 40),
            new Vector3(-9859, 1458, 41),
            new Vector3(-9869, 1456, 42),
            new Vector3(-9877, 1450, 43),
            new Vector3(-9887, 1452, 43),
            new Vector3(-9896, 1455, 42),
            new Vector3(-9906, 1455, 41),
            new Vector3(-9915, 1450, 41),
            new Vector3(-9923, 1445, 39),
            new Vector3(-9933, 1443, 39),
            new Vector3(-9942, 1449, 40),
            new Vector3(-9950, 1456, 41),
            new Vector3(-9960, 1457, 44),
            new Vector3(-9970, 1458, 45),
            new Vector3(-9980, 1458, 44),
            new Vector3(-9990, 1459, 42),
            new Vector3(-9999, 1464, 41),
            new Vector3(-10007, 1471, 41),
            new Vector3(-10010, 1481, 41),
            new Vector3(-10014, 1491, 41),
            new Vector3(-10014, 1501, 41),
            new Vector3(-10009, 1509, 43),
            new Vector3(-10005, 1518, 44),
            new Vector3(-10000, 1527, 43),
            new Vector3(-9995, 1536, 42),
            new Vector3(-9990, 1545, 42),
            new Vector3(-9985, 1554, 42),
            new Vector3(-9978, 1562, 43),
            new Vector3(-9970, 1567, 45),
            new Vector3(-9961, 1571, 45),
            new Vector3(-9952, 1575, 43),
            new Vector3(-9943, 1578, 41),
            new Vector3(-9934, 1582, 41),
            new Vector3(-9925, 1585, 44),
            new Vector3(-9917, 1591, 42),
            new Vector3(-9909, 1597, 41),
            new Vector3(-9900, 1602, 41),
            new Vector3(-9891, 1606, 40),
            new Vector3(-9884, 1614, 39),
            new Vector3(-9878, 1622, 38),
            new Vector3(-9873, 1631, 37),
            new Vector3(-9868, 1639, 35),
            new Vector3(-9865, 1648, 33),
            new Vector3(-9864, 1658, 35),
            new Vector3(-9867, 1667, 36),
            new Vector3(-9873, 1675, 36),
            new Vector3(-9880, 1682, 35),
            new Vector3(-9884, 1691, 32),
            new Vector3(-9886, 1701, 32),
            new Vector3(-9888, 1711, 34),
            new Vector3(-9887, 1721, 31),
            new Vector3(-9891, 1730, 30),
            new Vector3(-9896, 1736, 23),
            new Vector3(-9892, 1744, 19),
            new Vector3(-9891, 1753, 14),
            new Vector3(-9900, 1755, 11),
            new Vector3(-9910, 1755, 11),
            new Vector3(-9916, 1763, 12),
            new Vector3(-9922, 1771, 12),
            new Vector3(-9930, 1777, 13),
            new Vector3(-9938, 1782, 16),
            new Vector3(-9947, 1787, 16),
            new Vector3(-9956, 1791, 16),
            new Vector3(-9965, 1794, 17),
            new Vector3(-9970, 1803, 16),
            new Vector3(-9963, 1810, 14),
            new Vector3(-9956, 1817, 14),
            new Vector3(-9950, 1825, 14),
            new Vector3(-9948, 1835, 13),
            new Vector3(-9948, 1845, 13),
            new Vector3(-9951, 1855, 13),
            new Vector3(-9956, 1864, 14),
            new Vector3(-9960, 1873, 15),
            new Vector3(-9969, 1878, 14),
            new Vector3(-9979, 1880, 15),
            new Vector3(-9989, 1878, 14),
            new Vector3(-9998, 1874, 14),
            new Vector3(-10006, 1869, 17),
            new Vector3(-10016, 1866, 17),
            new Vector3(-10026, 1864, 18),
            new Vector3(-10035, 1860, 22),
            new Vector3(-10041, 1857, 29),
            new Vector3(-10049, 1855, 35),
            new Vector3(-10058, 1859, 37),
            new Vector3(-10067, 1864, 37),
            new Vector3(-10076, 1868, 36),
            new Vector3(-10086, 1869, 36),
            new Vector3(-10096, 1869, 36),
            new Vector3(-10106, 1870, 35),
            new Vector3(-10116, 1870, 34),
            new Vector3(-10126, 1872, 34),
            new Vector3(-10136, 1873, 34),
            new Vector3(-10146, 1873, 33),
            new Vector3(-10156, 1873, 33),
            new Vector3(-10166, 1872, 32),
            new Vector3(-10174, 1866, 34),
            new Vector3(-10183, 1860, 34),
            new Vector3(-10190, 1853, 34),
            new Vector3(-10194, 1844, 35),
            new Vector3(-10197, 1834, 37),
            new Vector3(-10199, 1824, 38),
            new Vector3(-10201, 1814, 37),
            new Vector3(-10209, 1808, 38),
            new Vector3(-10217, 1814, 38),
            new Vector3(-10217, 1824, 39),
            new Vector3(-10215, 1834, 39),
            new Vector3(-10213, 1844, 38),
            new Vector3(-10211, 1854, 37),
            new Vector3(-10210, 1864, 35),
            new Vector3(-10209, 1874, 36),
            new Vector3(-10210, 1884, 36),
            new Vector3(-10213, 1894, 36),
            new Vector3(-10214, 1904, 37),
            new Vector3(-10215, 1914, 37),
            new Vector3(-10215, 1924, 35),
            new Vector3(-10216, 1934, 34),
            new Vector3(-10214, 1943, 29),
            new Vector3(-10209, 1950, 23),
            new Vector3(-10206, 1958, 18),
            new Vector3(-10213, 1953, 23),
            new Vector3(-10219, 1950, 30),
            new Vector3(-10228, 1947, 34),
            new Vector3(-10238, 1944, 36),
            new Vector3(-10248, 1943, 37),
            new Vector3(-10258, 1941, 36),
            new Vector3(-10268, 1940, 35),
            new Vector3(-10278, 1941, 36),
            new Vector3(-10288, 1941, 35),
            new Vector3(-10297, 1937, 33),
            new Vector3(-10304, 1929, 35),
            new Vector3(-10311, 1922, 37),
            new Vector3(-10315, 1913, 38),
            new Vector3(-10317, 1903, 39),
            new Vector3(-10318, 1893, 39),
            new Vector3(-10318, 1883, 39),
            new Vector3(-10323, 1875, 37),
            new Vector3(-10330, 1868, 36),
            new Vector3(-10337, 1860, 37),
            new Vector3(-10344, 1852, 38),
            new Vector3(-10351, 1844, 38),
            new Vector3(-10357, 1836, 37),
            new Vector3(-10364, 1828, 37),
            new Vector3(-10371, 1820, 37),
            new Vector3(-10377, 1812, 37),
            new Vector3(-10383, 1804, 36),
            new Vector3(-10388, 1795, 36),
            new Vector3(-10391, 1786, 35),
            new Vector3(-10396, 1777, 35),
            new Vector3(-10403, 1770, 35),
            new Vector3(-10412, 1770, 31),
            new Vector3(-10415, 1779, 29),
            new Vector3(-10414, 1789, 27),
            new Vector3(-10411, 1798, 26),
            new Vector3(-10411, 1808, 24),
            new Vector3(-10408, 1817, 23),
            new Vector3(-10405, 1826, 22),
            new Vector3(-10403, 1836, 20),
            new Vector3(-10402, 1846, 19),
            new Vector3(-10403, 1856, 18),
            new Vector3(-10406, 1865, 17),
            new Vector3(-10413, 1872, 14),
            new Vector3(-10421, 1878, 12),
            new Vector3(-10428, 1885, 10),
            new Vector3(-10435, 1892, 6),
            new Vector3(-10441, 1900, 5),
            new Vector3(-10439, 1909, 9),
            new Vector3(-10432, 1917, 9),
            new Vector3(-10424, 1923, 7),
            new Vector3(-10418, 1931, 8),
            new Vector3(-10413, 1939, 10),
            new Vector3(-10409, 1948, 11),
            new Vector3(-10410, 1938, 10),
            new Vector3(-10416, 1930, 8),
            new Vector3(-10423, 1923, 7),
            new Vector3(-10431, 1918, 9),
            new Vector3(-10441, 1920, 10),
            new Vector3(-10449, 1926, 10),
            new Vector3(-10458, 1932, 9),
            new Vector3(-10466, 1938, 8),
            new Vector3(-10472, 1946, 10),
            new Vector3(-10465, 1954, 10),
            new Vector3(-10468, 1964, 9),
            new Vector3(-10474, 1973, 9),
            new Vector3(-10477, 1983, 10),
            new Vector3(-10474, 1993, 9),
            new Vector3(-10482, 1986, 10),
            new Vector3(-10484, 1976, 10),
            new Vector3(-10494, 1974, 11),
            new Vector3(-10503, 1978, 11),
            new Vector3(-10507, 1969, 9),
            new Vector3(-10512, 1961, 7),
            new Vector3(-10516, 1952, 5),
            new Vector3(-10524, 1946, 4),
            new Vector3(-10534, 1945, 3),
            new Vector3(-10543, 1948, 1),
            new Vector3(-10551, 1953, -1),
            new Vector3(-10558, 1959, -4),
            new Vector3(-10563, 1968, -4),
            new Vector3(-10568, 1977, -5),
            new Vector3(-10571, 1986, -7),
            new Vector3(-10567, 1995, -7),
            new Vector3(-10564, 2005, -7),
            new Vector3(-10554, 2007, -7),
            new Vector3(-10545, 2002, -8),
            new Vector3(-10554, 2006, -7),
            new Vector3(-10564, 2005, -7),
            new Vector3(-10573, 2002, -6),
            new Vector3(-10583, 2000, -6),
            new Vector3(-10592, 1996, -5),
            new Vector3(-10586, 1988, -7),
            new Vector3(-10580, 1981, -4),
            new Vector3(-10572, 1975, -5),
            new Vector3(-10567, 1967, -3),
            new Vector3(-10559, 1961, -4),
            new Vector3(-10551, 1955, -2),
            new Vector3(-10544, 1949, 1),
            new Vector3(-10535, 1945, 3),
            new Vector3(-10525, 1944, 4),
            new Vector3(-10516, 1950, 5),
            new Vector3(-10511, 1959, 7),
            new Vector3(-10506, 1967, 9),
            new Vector3(-10497, 1971, 11),
            new Vector3(-10487, 1974, 11),
            new Vector3(-10477, 1975, 9),
            new Vector3(-10470, 1968, 9),
            new Vector3(-10467, 1958, 9),
            new Vector3(-10470, 1949, 10),
            new Vector3(-10469, 1939, 8),
            new Vector3(-10462, 1932, 9),
            new Vector3(-10454, 1926, 9),
            new Vector3(-10445, 1920, 10),
            new Vector3(-10437, 1913, 10),
            new Vector3(-10430, 1907, 7),
            new Vector3(-10423, 1900, 7),
            new Vector3(-10417, 1893, 10),
            new Vector3(-10413, 1884, 12),
            new Vector3(-10409, 1875, 14),
            new Vector3(-10409, 1865, 17),
            new Vector3(-10409, 1855, 19),
            new Vector3(-10410, 1845, 20),
            new Vector3(-10411, 1835, 22),
            new Vector3(-10416, 1827, 24),
            new Vector3(-10420, 1818, 25),
            new Vector3(-10422, 1808, 26),
            new Vector3(-10425, 1799, 27),
            new Vector3(-10432, 1793, 30),
            new Vector3(-10439, 1798, 36),
            new Vector3(-10443, 1807, 37),
            new Vector3(-10447, 1816, 38),
            new Vector3(-10450, 1825, 39),
            new Vector3(-10454, 1835, 39),
            new Vector3(-10457, 1844, 40),
            new Vector3(-10461, 1854, 41),
            new Vector3(-10466, 1863, 44),
            new Vector3(-10473, 1869, 41),
            new Vector3(-10480, 1876, 41),
            new Vector3(-10487, 1884, 39),
            new Vector3(-10492, 1893, 39),
            new Vector3(-10493, 1903, 41),
            new Vector3(-10494, 1913, 41),
            new Vector3(-10497, 1922, 40),
            new Vector3(-10499, 1932, 42),
            new Vector3(-10502, 1941, 40),
            new Vector3(-10505, 1950, 38),
            new Vector3(-10508, 1960, 36),
            new Vector3(-10511, 1970, 36),
            new Vector3(-10514, 1979, 35),
            new Vector3(-10518, 1988, 34),
            new Vector3(-10524, 1996, 34),
            new Vector3(-10531, 2003, 35),
            new Vector3(-10540, 2008, 36),
            new Vector3(-10550, 2009, 35),
            new Vector3(-10560, 2010, 35),
            new Vector3(-10570, 2010, 36),
            new Vector3(-10580, 2010, 37),
            new Vector3(-10590, 2009, 39),
            new Vector3(-10599, 2006, 40),
            new Vector3(-10608, 2003, 39),
            new Vector3(-10617, 2000, 38),
            new Vector3(-10627, 1998, 36),
            new Vector3(-10636, 1995, 35),
            new Vector3(-10646, 1991, 34),
            new Vector3(-10656, 1987, 34),
            new Vector3(-10664, 1981, 34),
            new Vector3(-10673, 1977, 31),
            new Vector3(-10680, 1982, 26),
            new Vector3(-10681, 1991, 21),
            new Vector3(-10681, 2001, 18),
            new Vector3(-10689, 2006, 14),
            new Vector3(-10693, 1998, 18),
            new Vector3(-10694, 1988, 20),
            new Vector3(-10695, 1979, 24),
            new Vector3(-10695, 1970, 28),
            new Vector3(-10694, 1961, 31),
            new Vector3(-10694, 1951, 32),
            new Vector3(-10694, 1941, 33),
            new Vector3(-10704, 1940, 34),
            new Vector3(-10714, 1940, 38),
            new Vector3(-10724, 1940, 41),
            new Vector3(-10734, 1940, 42),
            new Vector3(-10744, 1940, 41),
            new Vector3(-10754, 1939, 39),
            new Vector3(-10764, 1939, 40),
            new Vector3(-10773, 1939, 44),
            new Vector3(-10782, 1938, 50),
            new Vector3(-10791, 1934, 54),
            new Vector3(-10799, 1929, 56),
            new Vector3(-10804, 1921, 58),
            new Vector3(-10809, 1913, 60),
            new Vector3(-10813, 1904, 62),
            new Vector3(-10807, 1895, 62),
            new Vector3(-10799, 1889, 59),
            new Vector3(-10796, 1880, 55),
            new Vector3(-10797, 1871, 52),
            new Vector3(-10799, 1861, 50),
            new Vector3(-10800, 1851, 49),
            new Vector3(-10802, 1841, 50),
            new Vector3(-10804, 1831, 51),
            new Vector3(-10812, 1825, 49),
            new Vector3(-10817, 1833, 52),
            new Vector3(-10823, 1840, 57),
            new Vector3(-10831, 1844, 62),
            new Vector3(-10837, 1850, 67),
            new Vector3(-10845, 1854, 73),
            new Vector3(-10855, 1851, 74),
            new Vector3(-10864, 1845, 73),
            new Vector3(-10872, 1839, 72),
            new Vector3(-10880, 1832, 72),
            new Vector3(-10888, 1826, 70),
            new Vector3(-10894, 1818, 68),
            new Vector3(-10899, 1809, 64),
            new Vector3(-10903, 1800, 62),
            new Vector3(-10907, 1791, 60),
            new Vector3(-10911, 1782, 59),
            new Vector3(-10918, 1775, 57),
            new Vector3(-10924, 1767, 55),
            new Vector3(-10930, 1759, 54),
            new Vector3(-10936, 1751, 53),
            new Vector3(-10942, 1743, 51),
            new Vector3(-10948, 1735, 49),
            new Vector3(-10952, 1726, 47),
            new Vector3(-10952, 1716, 45),
            new Vector3(-10953, 1706, 43),
            new Vector3(-10953, 1696, 43),
            new Vector3(-10953, 1686, 43),
            new Vector3(-10952, 1676, 43),
            new Vector3(-10950, 1666, 43),
            new Vector3(-10948, 1656, 45),
            new Vector3(-10946, 1646, 46),
            new Vector3(-10945, 1636, 47),
            new Vector3(-10943, 1626, 46),
            new Vector3(-10940, 1616, 45),
            new Vector3(-10938, 1606, 47),
            new Vector3(-10936, 1596, 49),
            new Vector3(-10933, 1586, 49),
            new Vector3(-10931, 1576, 49),
            new Vector3(-10929, 1566, 50),
            new Vector3(-10926, 1557, 49),
            new Vector3(-10923, 1548, 51),
            new Vector3(-10920, 1539, 53),
            new Vector3(-10917, 1530, 52),
            new Vector3(-10914, 1521, 50),
            new Vector3(-10911, 1512, 48),
            new Vector3(-10908, 1503, 46),
            new Vector3(-10905, 1494, 44),
            new Vector3(-10902, 1485, 42),
            new Vector3(-10899, 1476, 41),
            new Vector3(-10896, 1467, 43),
            new Vector3(-10893, 1458, 41),
            new Vector3(-10889, 1449, 42),
            new Vector3(-10884, 1440, 44),
            new Vector3(-10880, 1431, 45),
            new Vector3(-10875, 1422, 46),
            new Vector3(-10871, 1413, 46),
            new Vector3(-10867, 1404, 45),
            new Vector3(-10861, 1396, 43),
            new Vector3(-10853, 1390, 42),
            new Vector3(-10845, 1385, 40),
            new Vector3(-10836, 1380, 39),
            new Vector3(-10827, 1376, 37),
            new Vector3(-10818, 1380, 36),
            new Vector3(-10809, 1385, 33),
            new Vector3(-10801, 1390, 31),
            new Vector3(-10792, 1394, 28),
            new Vector3(-10784, 1398, 23),
            new Vector3(-10783, 1388, 25),
            new Vector3(-10788, 1381, 30),
            new Vector3(-10789, 1372, 33),
            new Vector3(-10785, 1363, 33),
            new Vector3(-10783, 1353, 34),
            new Vector3(-10784, 1343, 34),
            new Vector3(-10785, 1333, 34),
            new Vector3(-10787, 1323, 34),
            new Vector3(-10789, 1313, 33),
            new Vector3(-10791, 1303, 33),
            new Vector3(-10793, 1293, 33),
            new Vector3(-10795, 1283, 33),
            new Vector3(-10797, 1273, 35),
            new Vector3(-10797, 1263, 34),
            new Vector3(-10794, 1254, 33),
            new Vector3(-10786, 1247, 33),
            new Vector3(-10780, 1239, 34),
            new Vector3(-10775, 1231, 39),
            new Vector3(-10769, 1224, 44),
            new Vector3(-10763, 1216, 48),
            new Vector3(-10757, 1208, 52),
            new Vector3(-10751, 1200, 54),
            new Vector3(-10745, 1192, 55),
            new Vector3(-10739, 1184, 55),
            new Vector3(-10732, 1176, 56),
            new Vector3(-10727, 1167, 55),
            new Vector3(-10721, 1158, 53),
            new Vector3(-10716, 1150, 50),
            new Vector3(-10711, 1142, 46),
            new Vector3(-10706, 1134, 43),
            new Vector3(-10701, 1125, 43),
            new Vector3(-10696, 1116, 41),
            new Vector3(-10690, 1107, 41),
            new Vector3(-10685, 1098, 40),
            new Vector3(-10679, 1090, 40),
            new Vector3(-10673, 1082, 39),
            new Vector3(-10668, 1074, 37),
            new Vector3(-10663, 1066, 34),
            new Vector3(-10654, 1061, 33),
            new Vector3(-10645, 1056, 33),
            new Vector3(-10642, 1046, 33),
            new Vector3(-10639, 1036, 33),
            new Vector3(-10638, 1026, 34),
            new Vector3(-10636, 1016, 32),
            new Vector3(-10632, 1007, 32),
            new Vector3(-10624, 1001, 33),
            new Vector3(-10615, 998, 34),
            new Vector3(-10606, 994, 35),
            new Vector3(-10597, 990, 36),
            new Vector3(-10588, 987, 37),
            new Vector3(-10578, 985, 38),
            new Vector3(-10569, 981, 40),
            new Vector3(-10560, 978, 41),
            new Vector3(-10553, 971, 41),
            new Vector3(-10549, 962, 42),
            new Vector3(-10544, 954, 44),
            new Vector3(-10539, 945, 44),
            new Vector3(-10531, 939, 43),
            new Vector3(-10523, 945, 44),
            new Vector3(-10518, 953, 41),
            new Vector3(-10512, 961, 41),
            new Vector3(-10506, 968, 45),
            new Vector3(-10498, 975, 47),
            new Vector3(-10490, 981, 48),
            new Vector3(-10482, 987, 49),
            new Vector3(-10474, 993, 49),
            new Vector3(-10466, 998, 47),
            new Vector3(-10457, 997, 43),
            new Vector3(-10450, 990, 39),
            new Vector3(-10444, 983, 36),
            new Vector3(-10436, 977, 35),
            new Vector3(-10429, 970, 37),
            new Vector3(-10419, 967, 39),
            new Vector3(-10409, 965, 38),
            new Vector3(-10401, 959, 39),
            new Vector3(-10394, 951, 39),
            new Vector3(-10390, 942, 40),
            new Vector3(-10386, 933, 41),
            new Vector3(-10382, 924, 41),
            new Vector3(-10379, 915, 40),
            new Vector3(-10375, 906, 38),
            new Vector3(-10371, 897, 38),
            new Vector3(-10367, 888, 41),
            new Vector3(-10363, 879, 43),
            new Vector3(-10359, 870, 43),
            new Vector3(-10356, 860, 42),
            new Vector3(-10356, 850, 40),
            new Vector3(-10355, 840, 39),
            new Vector3(-10354, 830, 39),
            new Vector3(-10353, 820, 40),
            new Vector3(-10353, 810, 39),
            new Vector3(-10353, 800, 36),
            new Vector3(-10353, 790, 34),
            new Vector3(-10350, 781, 32),
            new Vector3(-10341, 776, 31),
            new Vector3(-10332, 780, 32),
            new Vector3(-10327, 787, 37),
            new Vector3(-10322, 795, 41),
            new Vector3(-10317, 802, 46),
            new Vector3(-10309, 807, 50),
            new Vector3(-10300, 810, 52),
            new Vector3(-10291, 813, 50),
            new Vector3(-10283, 816, 45),
            new Vector3(-10273, 818, 44),
            new Vector3(-10264, 821, 46),
            new Vector3(-10255, 824, 48),
            new Vector3(-10246, 827, 49),
            new Vector3(-10236, 828, 48),
            new Vector3(-10228, 826, 43),
            new Vector3(-10220, 822, 39),
            new Vector3(-10210, 818, 39),
            new Vector3(-10200, 817, 38),
            new Vector3(-10190, 816, 37),
            new Vector3(-10180, 814, 35),
            new Vector3(-10171, 813, 30),
            new Vector3(-10163, 811, 25),
            new Vector3(-10154, 809, 22),
            new Vector3(-10146, 803, 19),
            new Vector3(-10138, 797, 18),
            new Vector3(-10128, 797, 19),
            new Vector3(-10119, 801, 19),
            new Vector3(-10111, 806, 21),
            new Vector3(-10101, 808, 22),
            new Vector3(-10091, 808, 23),
            new Vector3(-10081, 808, 26),
            new Vector3(-10072, 811, 29),
            new Vector3(-10064, 818, 31),
            new Vector3(-10057, 825, 32),
            new Vector3(-10051, 833, 33),
            new Vector3(-10044, 841, 34),
            new Vector3(-10037, 848, 33),
            new Vector3(-10030, 856, 33),
            new Vector3(-10024, 864, 34),
            new Vector3(-10016, 871, 34),
            new Vector3(-10008, 875, 38),
            new Vector3(-10000, 870, 41),
            new Vector3(-9995, 862, 38),
            new Vector3(-9994, 853, 34),
            new Vector3(-9991, 844, 33),
            new Vector3(-9983, 837, 33),
            new Vector3(-9974, 841, 34),
            new Vector3(-9966, 847, 33),
            new Vector3(-9958, 853, 32),
            new Vector3(-9950, 859, 33),
            new Vector3(-9942, 866, 32),
            new Vector3(-9934, 873, 34),
            new Vector3(-9927, 880, 33),
            new Vector3(-9920, 887, 33),
            new Vector3(-9912, 893, 33),
            new Vector3(-9902, 891, 32),
            new Vector3(-9893, 887, 33),
            new Vector3(-9884, 882, 34),
            new Vector3(-9876, 876, 33),
            new Vector3(-9867, 872, 31),
            new Vector3(-9857, 871, 30),
            new Vector3(-9847, 872, 28),
            new Vector3(-9839, 878, 27),
            new Vector3(-9830, 884, 27),
            new Vector3(-9822, 877, 27),
            new Vector3(-9816, 869, 26),
            new Vector3(-9807, 866, 25),
        };

        public override string ToString()
        {
            return $"[{JobType}] (Copper, Tin, Silver) Westfall (Kamel)";
        }
    }
}