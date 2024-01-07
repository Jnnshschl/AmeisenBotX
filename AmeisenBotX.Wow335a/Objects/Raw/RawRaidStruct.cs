using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace AmeisenBotX.Wow335a.Objects.Raw
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RawRaidStruct
    {
        public nint RaidPlayer1 { get; set; }

        public nint RaidPlayer10 { get; set; }

        public nint RaidPlayer11 { get; set; }

        public nint RaidPlayer12 { get; set; }

        public nint RaidPlayer13 { get; set; }

        public nint RaidPlayer14 { get; set; }

        public nint RaidPlayer15 { get; set; }

        public nint RaidPlayer16 { get; set; }

        public nint RaidPlayer17 { get; set; }

        public nint RaidPlayer18 { get; set; }

        public nint RaidPlayer19 { get; set; }

        public nint RaidPlayer2 { get; set; }

        public nint RaidPlayer20 { get; set; }

        public nint RaidPlayer21 { get; set; }

        public nint RaidPlayer22 { get; set; }

        public nint RaidPlayer23 { get; set; }

        public nint RaidPlayer24 { get; set; }

        public nint RaidPlayer25 { get; set; }

        public nint RaidPlayer26 { get; set; }

        public nint RaidPlayer27 { get; set; }

        public nint RaidPlayer28 { get; set; }

        public nint RaidPlayer29 { get; set; }

        public nint RaidPlayer3 { get; set; }

        public nint RaidPlayer30 { get; set; }

        public nint RaidPlayer31 { get; set; }

        public nint RaidPlayer32 { get; set; }

        public nint RaidPlayer33 { get; set; }

        public nint RaidPlayer34 { get; set; }

        public nint RaidPlayer35 { get; set; }

        public nint RaidPlayer36 { get; set; }

        public nint RaidPlayer37 { get; set; }

        public nint RaidPlayer38 { get; set; }

        public nint RaidPlayer39 { get; set; }

        public nint RaidPlayer4 { get; set; }

        public nint RaidPlayer40 { get; set; }

        public nint RaidPlayer5 { get; set; }

        public nint RaidPlayer6 { get; set; }

        public nint RaidPlayer7 { get; set; }

        public nint RaidPlayer8 { get; set; }

        public nint RaidPlayer9 { get; set; }

        public readonly IEnumerable<nint> GetPointers()
        {
            return
            [
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
            ];
        }
    }
}