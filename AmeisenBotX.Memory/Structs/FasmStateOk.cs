using System.Runtime.InteropServices;

namespace AmeisenBotX.Memory.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct FasmStateOk
    {
        public int Condition { get; set; }

        public uint OutputLength { get; set; }

        public nint OutputData { get; set; }
    }
}