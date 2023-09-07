using System;
using System.Runtime.InteropServices;

namespace AmeisenBotX.Memory.Win32
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ProcessInformation
    {
        public IntPtr hProcess;
        public IntPtr hThread;
        public int dwProcessId;
        public int dwThreadId;
    }

}