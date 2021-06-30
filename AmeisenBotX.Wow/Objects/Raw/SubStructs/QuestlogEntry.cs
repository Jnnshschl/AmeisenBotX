using System.Runtime.InteropServices;

namespace AmeisenBotX.Wow.Objects.SubStructs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct QuestlogEntry
    {
        public int Id;
        public int Finished;
        public short ProgressPartymember1;
        public short ProgressPartymember2;
        public short ProgressPartymember3;
        public short ProgressPartymember4;
        public int Y;
    }
}