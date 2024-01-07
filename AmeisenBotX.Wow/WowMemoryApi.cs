using AmeisenBotX.Memory;
using AmeisenBotX.Wow.Offsets;

namespace AmeisenBotX.Wow
{
    public class WowMemoryApi(IOffsetList offsets) : XMemory()
    {
        public IOffsetList Offsets { get; } = offsets;
    }
}