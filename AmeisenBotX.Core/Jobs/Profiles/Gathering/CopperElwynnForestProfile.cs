﻿using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Jobs.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Jobs.Profiles.Gathering
{
    public class CopperElwynnForestProfile : IMiningProfile
    {
        public bool IsCirclePath => false;

        public JobType JobType => JobType.Mining;

        public List<Vector3> MailboxNodes { get; private set; } = new List<Vector3>()
        {
           new Vector3(-9456, 48, 56),
           //new Vector3(-9249, -2144, 64)
        };

        //public List<MailBox> MailBoxes { get; } = new List<MailBox>()
        //{
        //    MailBox.MailboxGoldShire
        //};

        public List<OreNodes> OreTypes { get; } = new List<OreNodes>()
        {
            OreNodes.Copper
        };

        public List<Vector3> Path { get; } = new List<Vector3>()
        {
            new Vector3(-9159, 357, 89),
            new Vector3(-9163, 350, 88),
            new Vector3(-9167, 343, 86),
            new Vector3(-9171, 336, 84),
            new Vector3(-9171, 328, 82),
            new Vector3(-9171, 320, 81),
            new Vector3(-9170, 312, 80),
            new Vector3(-9170, 304, 80),
            new Vector3(-9169, 296, 79),
            new Vector3(-9169, 288, 78),
            new Vector3(-9169, 280, 77),
            new Vector3(-9168, 272, 77),
            new Vector3(-9168, 264, 76),
            new Vector3(-9167, 256, 77),
            new Vector3(-9166, 248, 77),
            new Vector3(-9159, 243, 78),
            new Vector3(-9153, 243, 84),
            new Vector3(-9146, 243, 88),
            new Vector3(-9139, 242, 93),
            new Vector3(-9133, 242, 98),
            new Vector3(-9126, 242, 103),
            new Vector3(-9118, 241, 106),
            new Vector3(-9125, 241, 103),
            new Vector3(-9132, 242, 99),
            new Vector3(-9138, 242, 94),
            new Vector3(-9145, 243, 89),
            new Vector3(-9152, 243, 85),
            new Vector3(-9158, 242, 80),
            new Vector3(-9160, 235, 76),
            new Vector3(-9160, 227, 77),
            new Vector3(-9161, 219, 76),
            new Vector3(-9162, 211, 74),
            new Vector3(-9164, 203, 73),
            new Vector3(-9166, 195, 73),
            new Vector3(-9166, 187, 73),
            new Vector3(-9166, 179, 74),
            new Vector3(-9165, 171, 73),
            new Vector3(-9165, 163, 72),
            new Vector3(-9165, 138, 72),
            new Vector3(-9162, 131, 72),
            new Vector3(-9159, 123, 73),
            new Vector3(-9157, 116, 75),
            new Vector3(-9155, 108, 75),
            new Vector3(-9159, 101, 75),
            new Vector3(-9163, 94, 76),
            new Vector3(-9165, 86, 77),
            new Vector3(-9162, 78, 77),
            new Vector3(-9155, 75, 77),
            new Vector3(-9147, 73, 77),
            new Vector3(-9139, 70, 77),
            new Vector3(-9131, 68, 78),
            new Vector3(-9123, 69, 80),
            new Vector3(-9118, 73, 84),
            new Vector3(-9113, 77, 89),
            new Vector3(-9108, 80, 94),
            new Vector3(-9112, 76, 89),
            new Vector3(-9117, 72, 84),
            new Vector3(-9124, 70, 79),
            new Vector3(-9132, 68, 78),
            new Vector3(-9140, 67, 77),
            new Vector3(-9148, 65, 77),
            new Vector3(-9156, 66, 77),
            new Vector3(-9164, 67, 78),
            new Vector3(-9172, 70, 78),
            new Vector3(-9180, 72, 78),
            new Vector3(-9188, 74, 78),
            new Vector3(-9196, 74, 77),
            new Vector3(-9204, 74, 77),
            new Vector3(-9212, 74, 77),
            new Vector3(-9220, 74, 77),
            new Vector3(-9228, 74, 76),
            new Vector3(-9236, 75, 74),
            new Vector3(-9244, 77, 75),
            new Vector3(-9252, 79, 75),
            new Vector3(-9259, 83, 73),
            new Vector3(-9266, 88, 72),
            new Vector3(-9273, 92, 70),
            new Vector3(-9281, 95, 69),
            new Vector3(-9289, 97, 68),
            new Vector3(-9296, 99, 66),
            new Vector3(-9304, 100, 65),
            new Vector3(-9312, 100, 62),
            new Vector3(-9320, 100, 62),
            new Vector3(-9328, 100, 62),
            new Vector3(-9336, 101, 63),
            new Vector3(-9344, 101, 64),
            new Vector3(-9352, 102, 63),
            new Vector3(-9360, 104, 62),
            new Vector3(-9367, 107, 61),
            new Vector3(-9375, 108, 61),
            new Vector3(-9383, 110, 60),
            new Vector3(-9391, 113, 60),
            new Vector3(-9399, 116, 60),
            new Vector3(-9407, 119, 60),
            new Vector3(-9415, 122, 60),
            new Vector3(-9422, 125, 60),
            new Vector3(-9430, 127, 59),
            new Vector3(-9437, 130, 59),
            new Vector3(-9444, 133, 59),
            new Vector3(-9451, 137, 58),
            new Vector3(-9456, 143, 57),
            new Vector3(-9461, 149, 57),
            new Vector3(-9468, 153, 57),
            new Vector3(-9476, 152, 56),
            new Vector3(-9483, 157, 56),
            new Vector3(-9488, 164, 56),
            new Vector3(-9495, 168, 57),
            new Vector3(-9502, 171, 58),
            new Vector3(-9509, 174, 58),
            new Vector3(-9517, 175, 57),
            new Vector3(-9524, 180, 57),
            new Vector3(-9531, 184, 58),
            new Vector3(-9538, 187, 58),
            new Vector3(-9546, 188, 57),
            new Vector3(-9554, 190, 58),
            new Vector3(-9562, 190, 59),
            new Vector3(-9569, 193, 59),
            new Vector3(-9575, 188, 58),
            new Vector3(-9582, 186, 55),
            new Vector3(-9589, 187, 51),
            new Vector3(-9596, 187, 48),
            new Vector3(-9600, 180, 48),
            new Vector3(-9604, 173, 47),
            new Vector3(-9609, 167, 47),
            new Vector3(-9613, 160, 48),
            new Vector3(-9617, 154, 52),
            new Vector3(-9620, 148, 48),
            new Vector3(-9623, 141, 47),
            new Vector3(-9625, 133, 47),
            new Vector3(-9628, 126, 47),
            new Vector3(-9630, 118, 47),
            new Vector3(-9631, 110, 47),
            new Vector3(-9633, 102, 46),
            new Vector3(-9640, 98, 44),
            new Vector3(-9647, 95, 44),
            new Vector3(-9655, 92, 45),
            new Vector3(-9663, 90, 45),
            new Vector3(-9671, 88, 46),
            new Vector3(-9679, 88, 45),
            new Vector3(-9687, 88, 46),
            new Vector3(-9695, 89, 48),
            new Vector3(-9703, 90, 49),
            new Vector3(-9711, 90, 49),
            new Vector3(-9718, 93, 49),
            new Vector3(-9725, 97, 47),
            new Vector3(-9733, 98, 45),
            new Vector3(-9733, 106, 46),
            new Vector3(-9732, 114, 47),
            new Vector3(-9732, 122, 48),
            new Vector3(-9734, 130, 49),
            new Vector3(-9735, 138, 49),
            new Vector3(-9738, 146, 50),
            new Vector3(-9739, 154, 51),
            new Vector3(-9739, 162, 50),
            new Vector3(-9744, 168, 50),
            new Vector3(-9750, 173, 51),
            new Vector3(-9751, 181, 54),
            new Vector3(-9752, 189, 56),
            new Vector3(-9756, 194, 51),
            new Vector3(-9763, 196, 48),
            new Vector3(-9770, 199, 47),
            new Vector3(-9777, 201, 45),
            new Vector3(-9784, 204, 43),
            new Vector3(-9788, 211, 42),
            new Vector3(-9791, 218, 43),
            new Vector3(-9795, 225, 44),
            new Vector3(-9800, 231, 42),
            new Vector3(-9805, 237, 40),
            new Vector3(-9811, 242, 40),
            new Vector3(-9818, 247, 40),
            new Vector3(-9825, 251, 40),
            new Vector3(-9832, 255, 39),
            new Vector3(-9839, 256, 36),
            new Vector3(-9845, 252, 31),
            new Vector3(-9850, 247, 26),
            new Vector3(-9853, 240, 23),
            new Vector3(-9854, 233, 20),
            new Vector3(-9854, 226, 15),
            new Vector3(-9849, 220, 15),
            new Vector3(-9841, 217, 14),
            new Vector3(-9833, 214, 14),
            new Vector3(-9828, 208, 15),
            new Vector3(-9826, 200, 14),
            new Vector3(-9824, 192, 13),
            new Vector3(-9823, 184, 12),
            new Vector3(-9824, 176, 10),
            new Vector3(-9828, 170, 7),
            new Vector3(-9834, 165, 6),
            new Vector3(-9842, 164, 5),
            new Vector3(-9849, 159, 6),
            new Vector3(-9851, 151, 7),
            new Vector3(-9852, 143, 7),
            new Vector3(-9849, 136, 6),
            new Vector3(-9844, 129, 5),
            new Vector3(-9839, 123, 6),
            new Vector3(-9833, 118, 5),
            new Vector3(-9825, 120, 4),
            new Vector3(-9817, 120, 4),
            new Vector3(-9809, 119, 4),
            new Vector3(-9802, 117, 6),
            new Vector3(-9794, 115, 5),
            new Vector3(-9786, 113, 5),
            new Vector3(-9779, 108, 5),
            new Vector3(-9775, 101, 6),
            new Vector3(-9769, 96, 8),
            new Vector3(-9763, 92, 12),
            new Vector3(-9755, 90, 13),
            new Vector3(-9747, 90, 13),
            new Vector3(-9744, 97, 13),
            new Vector3(-9747, 104, 13),
            new Vector3(-9750, 111, 15),
            new Vector3(-9753, 118, 15),
            new Vector3(-9752, 126, 17),
            new Vector3(-9751, 133, 20),
            new Vector3(-9751, 141, 20),
            new Vector3(-9756, 148, 21),
            new Vector3(-9761, 154, 24),
            new Vector3(-9769, 154, 25),
            new Vector3(-9777, 154, 25),
            new Vector3(-9785, 152, 25),
            new Vector3(-9793, 151, 24),
            new Vector3(-9799, 156, 25),
            new Vector3(-9799, 164, 25),
            new Vector3(-9799, 172, 23),
            new Vector3(-9802, 180, 23),
            new Vector3(-9809, 184, 22),
            new Vector3(-9817, 183, 23),
            new Vector3(-9825, 181, 23),
            new Vector3(-9833, 182, 22),
            new Vector3(-9840, 186, 23),
            new Vector3(-9847, 184, 21),
            new Vector3(-9854, 181, 21),
            new Vector3(-9862, 179, 20),
            new Vector3(-9870, 179, 19),
            new Vector3(-9873, 172, 19),
            new Vector3(-9876, 166, 24),
            new Vector3(-9879, 161, 29),
            new Vector3(-9878, 153, 31),
            new Vector3(-9873, 147, 31),
            new Vector3(-9868, 141, 32),
            new Vector3(-9863, 135, 32),
            new Vector3(-9856, 131, 32),
            new Vector3(-9849, 126, 34),
            new Vector3(-9845, 120, 37),
            new Vector3(-9842, 113, 39),
            new Vector3(-9836, 107, 41),
            new Vector3(-9831, 101, 41),
            new Vector3(-9825, 96, 41),
            new Vector3(-9818, 93, 41),
            new Vector3(-9810, 91, 41),
            new Vector3(-9802, 89, 42),
            new Vector3(-9794, 88, 42),
            new Vector3(-9786, 86, 43),
            new Vector3(-9778, 85, 43),
            new Vector3(-9770, 84, 42),
            new Vector3(-9762, 82, 42),
            new Vector3(-9754, 80, 42),
            new Vector3(-9746, 78, 42),
            new Vector3(-9739, 75, 41),
            new Vector3(-9734, 68, 40),
            new Vector3(-9731, 61, 40),
            new Vector3(-9725, 55, 41),
            new Vector3(-9722, 48, 41),
            new Vector3(-9718, 41, 41),
            new Vector3(-9714, 34, 42),
            new Vector3(-9707, 30, 43),
            new Vector3(-9700, 28, 46),
            new Vector3(-9694, 24, 50),
            new Vector3(-9687, 20, 49),
            new Vector3(-9681, 15, 47),
            new Vector3(-9674, 11, 47),
            new Vector3(-9666, 9, 46),
            new Vector3(-9658, 9, 46),
            new Vector3(-9650, 7, 45),
            new Vector3(-9643, 2, 45),
            new Vector3(-9636, -3, 45),
            new Vector3(-9629, -7, 47),
            new Vector3(-9622, -10, 50),
            new Vector3(-9621, -11, 50),
            new Vector3(-9610, -12, 58),
            new Vector3(-9606, -16, 59),
            new Vector3(-9601, -30, 59),
            new Vector3(-9598, -37, 60),
            new Vector3(-9595, -44, 59),
            new Vector3(-9595, -52, 60),
            new Vector3(-9597, -60, 60),
            new Vector3(-9600, -67, 60),
            new Vector3(-9603, -74, 60),
            new Vector3(-9605, -82, 61),
            new Vector3(-9607, -90, 61),
            new Vector3(-9608, -98, 60),
            new Vector3(-9608, -106, 60),
            new Vector3(-9609, -114, 60),
            new Vector3(-9609, -122, 60),
            new Vector3(-9610, -130, 59),
            new Vector3(-9612, -138, 59),
            new Vector3(-9614, -145, 57),
            new Vector3(-9616, -153, 57),
            new Vector3(-9618, -160, 55),
            new Vector3(-9620, -168, 56),
            new Vector3(-9623, -175, 56),
            new Vector3(-9628, -181, 56),
            new Vector3(-9632, -188, 55),
            new Vector3(-9637, -194, 55),
            new Vector3(-9642, -201, 54),
            new Vector3(-9646, -208, 52),
            new Vector3(-9646, -215, 56),
            new Vector3(-9646, -222, 59),
            new Vector3(-9650, -229, 60),
            new Vector3(-9655, -236, 61),
            new Vector3(-9660, -242, 61),
            new Vector3(-9664, -249, 61),
            new Vector3(-9666, -256, 63),
            new Vector3(-9666, -264, 64),
            new Vector3(-9664, -271, 62),
            new Vector3(-9660, -278, 61),
            new Vector3(-9657, -285, 59),
            new Vector3(-9654, -293, 58),
            new Vector3(-9651, -301, 59),
            new Vector3(-9647, -308, 60),
            new Vector3(-9643, -315, 60),
            new Vector3(-9639, -322, 59),
            new Vector3(-9635, -329, 59),
            new Vector3(-9630, -336, 58),
            new Vector3(-9625, -342, 58),
            new Vector3(-9618, -346, 57),
            new Vector3(-9611, -349, 58),
            new Vector3(-9604, -352, 59),
            new Vector3(-9599, -358, 61),
            new Vector3(-9595, -365, 62),
            new Vector3(-9590, -372, 62),
            new Vector3(-9586, -379, 62),
            new Vector3(-9583, -386, 63),
            new Vector3(-9578, -392, 64),
            new Vector3(-9574, -399, 64),
            new Vector3(-9569, -405, 64),
            new Vector3(-9565, -412, 64),
            new Vector3(-9562, -419, 64),
            new Vector3(-9558, -426, 63),
            new Vector3(-9554, -433, 61),
            new Vector3(-9551, -440, 60),
            new Vector3(-9546, -447, 60),
            new Vector3(-9541, -454, 60),
            new Vector3(-9536, -460, 61),
            new Vector3(-9531, -466, 61),
            new Vector3(-9526, -473, 61),
            new Vector3(-9521, -480, 61),
            new Vector3(-9516, -486, 62),
            new Vector3(-9509, -489, 62),
            new Vector3(-9501, -491, 62),
            new Vector3(-9493, -492, 62),
            new Vector3(-9489, -499, 63),
            new Vector3(-9487, -507, 63),
            new Vector3(-9484, -515, 63),
            new Vector3(-9479, -521, 64),
            new Vector3(-9475, -528, 64),
            new Vector3(-9471, -535, 66),
            new Vector3(-9466, -541, 66),
            new Vector3(-9460, -547, 66),
            new Vector3(-9453, -552, 66),
            new Vector3(-9446, -555, 68),
            new Vector3(-9439, -558, 69),
            new Vector3(-9431, -559, 71),
            new Vector3(-9423, -559, 72),
            new Vector3(-9415, -559, 72),
            new Vector3(-9408, -564, 71),
            new Vector3(-9407, -572, 70),
            new Vector3(-9408, -579, 67),
            new Vector3(-9408, -587, 68),
            new Vector3(-9407, -595, 68),
            new Vector3(-9408, -603, 68),
            new Vector3(-9408, -611, 69),
            new Vector3(-9409, -618, 73),
            new Vector3(-9410, -626, 72),
            new Vector3(-9412, -633, 68),
            new Vector3(-9413, -626, 71),
            new Vector3(-9408, -620, 73),
            new Vector3(-9401, -619, 69),
            new Vector3(-9393, -618, 69),
            new Vector3(-9385, -615, 69),
            new Vector3(-9379, -610, 69),
            new Vector3(-9372, -607, 69),
            new Vector3(-9365, -604, 70),
            new Vector3(-9358, -601, 70),
            new Vector3(-9351, -597, 71),
            new Vector3(-9343, -595, 71),
            new Vector3(-9335, -593, 71),
            new Vector3(-9327, -591, 70),
            new Vector3(-9319, -590, 70),
            new Vector3(-9311, -588, 70),
            new Vector3(-9303, -587, 68),
            new Vector3(-9296, -585, 66),
            new Vector3(-9288, -584, 65),
            new Vector3(-9280, -584, 65),
            new Vector3(-9273, -587, 65),
            new Vector3(-9266, -590, 65),
            new Vector3(-9260, -595, 64),
            new Vector3(-9254, -600, 64),
            new Vector3(-9248, -605, 64),
            new Vector3(-9241, -609, 63),
            new Vector3(-9233, -610, 62),
            new Vector3(-9225, -610, 61),
            new Vector3(-9217, -611, 61),
            new Vector3(-9209, -611, 60),
            new Vector3(-9201, -611, 60),
            new Vector3(-9193, -609, 61),
            new Vector3(-9186, -606, 63),
            new Vector3(-9178, -603, 63),
            new Vector3(-9171, -600, 63),
            new Vector3(-9164, -597, 61),
            new Vector3(-9157, -594, 59),
            new Vector3(-9149, -595, 58),
            new Vector3(-9141, -596, 58),
            new Vector3(-9133, -594, 58),
            new Vector3(-9127, -589, 58),
            new Vector3(-9123, -582, 59),
            new Vector3(-9120, -575, 59),
            new Vector3(-9115, -568, 59),
            new Vector3(-9107, -566, 61),
            new Vector3(-9099, -565, 62),
            new Vector3(-9092, -561, 62),
            new Vector3(-9086, -556, 61),
            new Vector3(-9080, -550, 60),
            new Vector3(-9073, -547, 59),
            new Vector3(-9065, -547, 58),
            new Vector3(-9057, -550, 57),
            new Vector3(-9049, -552, 56),
            new Vector3(-9042, -555, 55),
            new Vector3(-9035, -558, 55),
            new Vector3(-9029, -563, 55),
            new Vector3(-9026, -570, 55),
            new Vector3(-9029, -578, 56),
            new Vector3(-9032, -585, 56),
            new Vector3(-9033, -593, 56),
            new Vector3(-9036, -600, 56),
            new Vector3(-9041, -605, 53),
            new Vector3(-9047, -602, 57),
            new Vector3(-9053, -596, 58),
            new Vector3(-9061, -597, 59),
            new Vector3(-9068, -599, 61),
            new Vector3(-9074, -594, 62),
            new Vector3(-9078, -587, 62),
            new Vector3(-9083, -581, 62),
            new Vector3(-9088, -574, 62),
            new Vector3(-9092, -567, 62),
            new Vector3(-9099, -563, 62),
            new Vector3(-9107, -564, 61),
            new Vector3(-9114, -567, 59),
            new Vector3(-9119, -573, 59),
            new Vector3(-9123, -580, 59),
            new Vector3(-9127, -587, 58),
            new Vector3(-9133, -592, 58),
            new Vector3(-9141, -594, 58),
            new Vector3(-9149, -594, 58),
            new Vector3(-9157, -594, 59),
            new Vector3(-9164, -596, 61),
            new Vector3(-9171, -599, 63),
            new Vector3(-9178, -603, 63),
            new Vector3(-9185, -606, 63),
            new Vector3(-9192, -608, 61),
            new Vector3(-9200, -611, 61),
            new Vector3(-9207, -614, 60),
            new Vector3(-9214, -617, 61),
            new Vector3(-9221, -621, 61),
            new Vector3(-9228, -624, 62),
            new Vector3(-9235, -627, 62),
            new Vector3(-9242, -631, 63),
        };

        public override string ToString()
        {
            return $"[{JobType}] (Copper) Elwynn Forest (Kamel)";
        }
    }
}