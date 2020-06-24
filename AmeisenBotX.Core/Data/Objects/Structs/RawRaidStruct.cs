using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace AmeisenBotX.Core.Data.Objects.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RawRaidStruct
    {
        public RawRaidPlayer RaidPlayer1 { get; set; }

        public RawRaidPlayer RaidPlayer10 { get; set; }

        public RawRaidPlayer RaidPlayer11 { get; set; }

        public RawRaidPlayer RaidPlayer12 { get; set; }

        public RawRaidPlayer RaidPlayer13 { get; set; }

        public RawRaidPlayer RaidPlayer14 { get; set; }

        public RawRaidPlayer RaidPlayer15 { get; set; }

        public RawRaidPlayer RaidPlayer16 { get; set; }

        public RawRaidPlayer RaidPlayer17 { get; set; }

        public RawRaidPlayer RaidPlayer18 { get; set; }

        public RawRaidPlayer RaidPlayer19 { get; set; }

        public RawRaidPlayer RaidPlayer2 { get; set; }

        public RawRaidPlayer RaidPlayer20 { get; set; }

        public RawRaidPlayer RaidPlayer21 { get; set; }

        public RawRaidPlayer RaidPlayer22 { get; set; }

        public RawRaidPlayer RaidPlayer23 { get; set; }

        public RawRaidPlayer RaidPlayer24 { get; set; }

        public RawRaidPlayer RaidPlayer25 { get; set; }

        public RawRaidPlayer RaidPlayer26 { get; set; }

        public RawRaidPlayer RaidPlayer27 { get; set; }

        public RawRaidPlayer RaidPlayer28 { get; set; }

        public RawRaidPlayer RaidPlayer29 { get; set; }

        public RawRaidPlayer RaidPlayer3 { get; set; }

        public RawRaidPlayer RaidPlayer30 { get; set; }

        public RawRaidPlayer RaidPlayer31 { get; set; }

        public RawRaidPlayer RaidPlayer32 { get; set; }

        public RawRaidPlayer RaidPlayer33 { get; set; }

        public RawRaidPlayer RaidPlayer34 { get; set; }

        public RawRaidPlayer RaidPlayer35 { get; set; }

        public RawRaidPlayer RaidPlayer36 { get; set; }

        public RawRaidPlayer RaidPlayer37 { get; set; }

        public RawRaidPlayer RaidPlayer38 { get; set; }

        public RawRaidPlayer RaidPlayer39 { get; set; }

        public RawRaidPlayer RaidPlayer4 { get; set; }

        public RawRaidPlayer RaidPlayer40 { get; set; }

        public RawRaidPlayer RaidPlayer5 { get; set; }

        public RawRaidPlayer RaidPlayer6 { get; set; }

        public RawRaidPlayer RaidPlayer7 { get; set; }

        public RawRaidPlayer RaidPlayer8 { get; set; }

        public RawRaidPlayer RaidPlayer9 { get; set; }

        public List<ulong> GetGuids()
        {
            return new List<ulong>()
            {
                RaidPlayer1.Guid,
                RaidPlayer2.Guid,
                RaidPlayer3.Guid,
                RaidPlayer4.Guid,
                RaidPlayer5.Guid,
                RaidPlayer6.Guid,
                RaidPlayer7.Guid,
                RaidPlayer8.Guid,
                RaidPlayer9.Guid,
                RaidPlayer10.Guid,
                RaidPlayer11.Guid,
                RaidPlayer12.Guid,
                RaidPlayer13.Guid,
                RaidPlayer14.Guid,
                RaidPlayer15.Guid,
                RaidPlayer16.Guid,
                RaidPlayer17.Guid,
                RaidPlayer18.Guid,
                RaidPlayer19.Guid,
                RaidPlayer20.Guid,
                RaidPlayer21.Guid,
                RaidPlayer22.Guid,
                RaidPlayer23.Guid,
                RaidPlayer24.Guid,
                RaidPlayer25.Guid,
                RaidPlayer26.Guid,
                RaidPlayer27.Guid,
                RaidPlayer28.Guid,
                RaidPlayer29.Guid,
                RaidPlayer30.Guid,
                RaidPlayer31.Guid,
                RaidPlayer32.Guid,
                RaidPlayer33.Guid,
                RaidPlayer34.Guid,
                RaidPlayer35.Guid,
                RaidPlayer36.Guid,
                RaidPlayer37.Guid,
                RaidPlayer38.Guid,
                RaidPlayer39.Guid,
                RaidPlayer40.Guid
            };
        }
    }
}