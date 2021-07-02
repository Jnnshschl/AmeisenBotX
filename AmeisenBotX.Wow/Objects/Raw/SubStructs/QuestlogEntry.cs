using System.Runtime.InteropServices;

namespace AmeisenBotX.Wow.Objects.SubStructs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct QuestlogEntry
    {
        public int Id { get; set; }

        public int Finished { get; set; }

        public short ProgressPartymember1 { get; set; }

        public short ProgressPartymember2 { get; set; }

        public short ProgressPartymember3 { get; set; }

        public short ProgressPartymember4 { get; set; }

        public int Y { get; set; }
    }
}