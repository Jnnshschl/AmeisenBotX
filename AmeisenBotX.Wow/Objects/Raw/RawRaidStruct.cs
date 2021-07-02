using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace AmeisenBotX.Wow.Objects
{
    [StructLayout(LayoutKind.Sequential)]
    public record RawRaidStruct
    {
        public IntPtr RaidPlayer1 { get; set; }

        public IntPtr RaidPlayer10 { get; set; }

        public IntPtr RaidPlayer11 { get; set; }

        public IntPtr RaidPlayer12 { get; set; }

        public IntPtr RaidPlayer13 { get; set; }

        public IntPtr RaidPlayer14 { get; set; }

        public IntPtr RaidPlayer15 { get; set; }

        public IntPtr RaidPlayer16 { get; set; }

        public IntPtr RaidPlayer17 { get; set; }

        public IntPtr RaidPlayer18 { get; set; }

        public IntPtr RaidPlayer19 { get; set; }

        public IntPtr RaidPlayer2 { get; set; }

        public IntPtr RaidPlayer20 { get; set; }

        public IntPtr RaidPlayer21 { get; set; }

        public IntPtr RaidPlayer22 { get; set; }

        public IntPtr RaidPlayer23 { get; set; }

        public IntPtr RaidPlayer24 { get; set; }

        public IntPtr RaidPlayer25 { get; set; }

        public IntPtr RaidPlayer26 { get; set; }

        public IntPtr RaidPlayer27 { get; set; }

        public IntPtr RaidPlayer28 { get; set; }

        public IntPtr RaidPlayer29 { get; set; }

        public IntPtr RaidPlayer3 { get; set; }

        public IntPtr RaidPlayer30 { get; set; }

        public IntPtr RaidPlayer31 { get; set; }

        public IntPtr RaidPlayer32 { get; set; }

        public IntPtr RaidPlayer33 { get; set; }

        public IntPtr RaidPlayer34 { get; set; }

        public IntPtr RaidPlayer35 { get; set; }

        public IntPtr RaidPlayer36 { get; set; }

        public IntPtr RaidPlayer37 { get; set; }

        public IntPtr RaidPlayer38 { get; set; }

        public IntPtr RaidPlayer39 { get; set; }

        public IntPtr RaidPlayer4 { get; set; }

        public IntPtr RaidPlayer40 { get; set; }

        public IntPtr RaidPlayer5 { get; set; }

        public IntPtr RaidPlayer6 { get; set; }

        public IntPtr RaidPlayer7 { get; set; }

        public IntPtr RaidPlayer8 { get; set; }

        public IntPtr RaidPlayer9 { get; set; }

        public IEnumerable<IntPtr> GetPointers()
        {
            return new List<IntPtr>()
            {
                RaidPlayer1,
                RaidPlayer2,
                RaidPlayer3,
                RaidPlayer4,
                RaidPlayer5,
                RaidPlayer6,
                RaidPlayer7,
                RaidPlayer8,
                RaidPlayer9,
                RaidPlayer10,
                RaidPlayer11,
                RaidPlayer12,
                RaidPlayer13,
                RaidPlayer14,
                RaidPlayer15,
                RaidPlayer16,
                RaidPlayer17,
                RaidPlayer18,
                RaidPlayer19,
                RaidPlayer20,
                RaidPlayer21,
                RaidPlayer22,
                RaidPlayer23,
                RaidPlayer24,
                RaidPlayer25,
                RaidPlayer26,
                RaidPlayer27,
                RaidPlayer28,
                RaidPlayer29,
                RaidPlayer30,
                RaidPlayer31,
                RaidPlayer32,
                RaidPlayer33,
                RaidPlayer34,
                RaidPlayer35,
                RaidPlayer36,
                RaidPlayer37,
                RaidPlayer38,
                RaidPlayer39,
                RaidPlayer40
            };
        }
    }
}