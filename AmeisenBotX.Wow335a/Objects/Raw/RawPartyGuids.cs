using System.Runtime.InteropServices;

namespace AmeisenBotX.Wow335a.Objects.Raw
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RawPartyGuids
    {
        public ulong PartymemberGuid1 { get; set; }

        public ulong PartymemberGuid2 { get; set; }

        public ulong PartymemberGuid3 { get; set; }

        public ulong PartymemberGuid4 { get; set; }

        public readonly ulong[] AsArray()
        {
            return
            [
                PartymemberGuid1,
                PartymemberGuid2,
                PartymemberGuid3,
                PartymemberGuid4,
            ];
        }
    }
}