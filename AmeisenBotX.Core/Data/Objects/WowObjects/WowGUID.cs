namespace AmeisenBotX.Core.Data.Objects.WowObjects
{
    public class WowGUID
    {
        public static int NpcId(ulong guid)
        {
            return (int)((guid >> 24) & 0x0000000000FFFFFF);
        }
    }
}