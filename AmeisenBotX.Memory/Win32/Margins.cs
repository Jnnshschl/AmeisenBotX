using System.Runtime.InteropServices;

namespace AmeisenBotX.Memory.Win32
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Margins
    {
        public int Left, Right, Top, Bottom;
    }
}