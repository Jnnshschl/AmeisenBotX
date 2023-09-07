using System;
using System.Runtime.InteropServices;
using AmeisenBotX.Common.Memory;
using AmeisenBotX.Common.Memory.Enums;

namespace AmeisenBotX.Memory.Win32
{
    public static unsafe class Win32Imports
    {
        public const int GWL_EXSTYLE = -0x14;
        public const int GWL_STYLE = -16;
        public const int STARTF_USESHOWWINDOW = 1;
        public const int SW_SHOWMINNOACTIVE = 7;
        public const int SW_SHOWNOACTIVATE = 4;
        public const int SWP_NOACTIVATE = 0x10;
        public const int SWP_NOZORDER = 0x4;

        [DllImport("user32", SetLastError = true)]
        public static extern bool GetWindowRect(IntPtr windowHandle, ref Rect rectangle);

        [DllImport("kernel32", SetLastError = true)]
        internal static extern bool CloseHandle(IntPtr threadHandle);

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool CreateProcess
        (
            string lpApplicationName,
            string lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            [In] ref StartupInfo lpStartupInfo,
            out ProcessInformation lpProcessInformation
        );

        [DllImport("user32", SetLastError = true)]
        internal static extern bool GetClientRect(IntPtr windowHandle, ref Rect rectangle);

        [DllImport("user32", SetLastError = true)]
        internal static extern IntPtr GetForegroundWindow();

        [DllImport("user32", SetLastError = true)]
        internal static extern int GetWindowLong(IntPtr windowHandle, int index);

        [DllImport("user32", SetLastError = true)]
        internal static extern int GetWindowThreadProcessId(IntPtr windowHandle, int processId);

        [DllImport("ntdll", SetLastError = true)]
        internal static extern bool NtReadVirtualMemory(IntPtr processHandle, IntPtr baseAddress, void* buffer, int size, out IntPtr numberOfBytesRead);

        [DllImport("ntdll", SetLastError = true)]
        internal static extern bool NtResumeThread(IntPtr threadHandle, out IntPtr suspendCount);

        [DllImport("ntdll", SetLastError = true)]
        internal static extern bool NtSuspendThread(IntPtr threadHandle, out IntPtr previousSuspendCount);

        [DllImport("ntdll", SetLastError = true)]
        internal static extern bool NtWriteVirtualMemory(IntPtr processHandle, IntPtr baseAddress, void* buffer, int size, out IntPtr numberOfBytesWritten);

        [DllImport("kernel32", SetLastError = true)]
        public static extern IntPtr OpenProcess(ProcessAccessFlag processAccess, bool inheritHandle, int processId);

        [DllImport("kernel32", SetLastError = true)]
        internal static extern IntPtr OpenThread(ThreadAccessFlag threadAccess, bool inheritHandle, uint threadId);

        [DllImport("user32", SetLastError = true)]
        internal static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32", SetLastError = true)]
        internal static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32", SetLastError = true)]
        internal static extern int SetWindowLong(IntPtr windowHandle, int index, int newLong);

        [DllImport("user32", SetLastError = true)]
        internal static extern IntPtr SetWindowPos(IntPtr windowHandle, IntPtr windowHandleInsertAfter, int x, int y, int cx, int cy, int wFlags);

        [DllImport("kernel32", SetLastError = true)]
        internal static extern IntPtr VirtualAllocEx(IntPtr processHandle, IntPtr address, uint size, AllocationType allocationType, MemoryProtectionFlag memoryProtection);

        [DllImport("kernel32", SetLastError = true)]
        internal static extern bool VirtualFreeEx(IntPtr processHandle, IntPtr address, int size, AllocationType allocationType);

        [DllImport("kernel32", SetLastError = true)]
        internal static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, MemoryProtectionFlag flNewProtect, out MemoryProtectionFlag lpflOldProtect);
    }
}