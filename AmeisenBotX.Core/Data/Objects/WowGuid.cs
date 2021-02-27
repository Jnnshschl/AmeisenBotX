namespace AmeisenBotX.Core.Data.Objects
{
    public class WowGuid
    {
        public static int ToNpcId(ulong guid)
        {
            return (int)((guid >> 24) & 0x0000000000FFFFFF);
        }
    }
}