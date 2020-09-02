using System.Runtime.InteropServices;

namespace AmeisenBotX.Core.Data.Objects.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RawPartyGuids
    {
        public ulong PartymemberGuid1 { get; set; }

        public ulong PartymemberGuid2 { get; set; }

        public ulong PartymemberGuid3 { get; set; }

        public ulong PartymemberGuid4 { get; set; }

        public ulong[] AsArray()
        {
            return new ulong[]
            {
                PartymemberGuid1,
                PartymemberGuid2,
                PartymemberGuid3,
                PartymemberGuid4,
            };
        }
    }
}