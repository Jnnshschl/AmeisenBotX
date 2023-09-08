using AmeisenBotX.Memory;
using AmeisenBotX.Wow.Offsets;

namespace AmeisenBotX.Wow
{
    public class WowMemoryApi : XMemory
    {
        public WowMemoryApi(IOffsetList offsets)
            : base()
        {
            Offsets = offsets;
        }
    }
}