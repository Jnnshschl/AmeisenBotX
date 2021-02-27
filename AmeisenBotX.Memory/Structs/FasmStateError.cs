using System.Runtime.InteropServices;

namespace AmeisenBotX.Memory.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct FasmStateError
    {
        public int Condition { get; set; }

        public int ErrorCode { get; set; }
    }
}