using System.Collections.Specialized;
using System.Runtime.InteropServices;

namespace AmeisenBotX.Wow548.Objects.Descriptors
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct WowItemDescriptor548
    {
        public ulong Owner;
        public ulong ContainedIn;
        public ulong Creator;
        public ulong GiftCreator;
        public int StackCount;
        public int Expiration;
        public fixed int SpellCharges[5];
        public BitVector32 DynamicFlags;
        public fixed int Enchantment[39];
        public int PropertySeed;
        public int RandomPropertiesId;
        public int Durability;
        public int MaxDurability;
        public int CreatePlayedTime;
        public int ModifiersMask;
    }
}