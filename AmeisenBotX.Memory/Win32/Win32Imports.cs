using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AmeisenBotX.Memory.Win32
{
    public unsafe class Win32Imports
    {
        public const int STARTF_USESHOWWINDOW = 1;
        public const int SW_SHOWMINNOACTIVE = 7;
        public const int SW_SHOWNOACTIVATE = 4;

        public const int GWL_EXSTYLE = -0x14;
        public const int GWL_STYLE = -16;
        public const int WS_CHILD = 0x40000000;
        public const int WS_EX_APPWINDOW = 0x40000;
        public const int WS_EX_NOACTIVATE = 0x08000000;

        [Flags]
        public enum AllocationType : uint
        {
            Commit = 0x1000,
            Reserve = 0x2000,
            Decommit = 0x4000,
            Release = 0x8000,
            Reset = 0x80000,
            Physical = 0x400000,
            TopDown = 0x100000,
            WriteWatch = 0x200000,
            LargePages = 0x20000000
        }

        [Flags]
        public enum MemoryProtection : uint
        {
            NoAccess = 0x1,
            ReadOnly = 0x2,
            ReadWrite = 0x4,
            WriteCopy = 0x8,
            Execute = 0x10,
            ExecuteRead = 0x20,
            ExecuteReadWrite = 0x40,
            ExecuteWriteCopy = 0x80,
            GuardModifierflag = 0x100,
            NoCacheModifierflag = 0x200,
            WriteCombineModifierflag = 0x400
        }

        [Flags]
        public enum ProcessAccessFlags : uint
        {
            All = 0x1F0FFF,
            Terminate = 0x1,
            CreateThread = 0x2,
            VirtualMemoryOperation = 0x8,
            VirtualMemoryRead = 0x10,
            VirtualMemoryWrite = 0x20,
            DuplicateHandle = 0x40,
            CreateProcess = 0x80,
            SetQuota = 0x100,
            SetInformation = 0x200,
            QueryInformation = 0x400,
            QueryLimitedInformation = 0x1000,
            Synchronize = 0x100000
        }

        [Flags]
        public enum ThreadAccess : uint
        {
            Terminate = 0x1,
            SuspendResume = 0x2,
            GetContext = 0x8,
            SetContext = 0x10,
            SetInformation = 0x20,
            QueryInformation = 0x40,
            SetThreadToken = 0x80,
            Impersonate = 0x100,
            DirectImpersonation = 0x200
        }

        [Flags]
        public enum WindowFlags : uint
        {
            NoSize = 0x1,
            NoMove = 0x2,
            NoZOrder = 0x4,
            NoRedraw = 0x8,
            NoActivate = 0x10,
            DrawFrame = 0x20,
            FrameChanged = 0x20,
            ShowWindow = 0x40,
            HideWindow = 0x80,
            NoCopyBits = 0x100,
            NoOwnerZOrder = 0x200,
            NoReposition = 0x200,
            NoSendChanging = 0x400,
            Defererase = 0x2000,
            AsyncWindowPos = 0x4000
        }

        [Flags]
        public enum WindowStyles : uint
        {
            WS_OVERLAPPED = 0x00000000,
            WS_POPUP = 0x80000000,
            WS_CHILD = 0x40000000,
            WS_MINIMIZE = 0x20000000,
            WS_VISIBLE = 0x10000000,
            WS_DISABLED = 0x08000000,
            WS_CLIPSIBLINGS = 0x04000000,
            WS_CLIPCHILDREN = 0x02000000,
            WS_MAXIMIZE = 0x01000000,
            WS_BORDER = 0x00800000,
            WS_DLGFRAME = 0x00400000,
            WS_VSCROLL = 0x00200000,
            WS_HSCROLL = 0x00100000,
            WS_SYSMENU = 0x00080000,
            WS_THICKFRAME = 0x00040000,
            WS_GROUP = 0x00020000,
            WS_TABSTOP = 0x00010000,

            WS_MINIMIZEBOX = 0x00020000,
            WS_MAXIMIZEBOX = 0x00010000,

            WS_CAPTION = WS_BORDER | WS_DLGFRAME,
            WS_TILED = WS_OVERLAPPED,
            WS_ICONIC = WS_MINIMIZE,
            WS_SIZEBOX = WS_THICKFRAME,
            WS_TILEDWINDOW = WS_OVERLAPPEDWINDOW,

            WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,
            WS_POPUPWINDOW = WS_POPUP | WS_BORDER | WS_SYSMENU,
            WS_CHILDWINDOW = WS_CHILD,

            WS_EX_DLGMODALFRAME = 0x00000001,
            WS_EX_NOPARENTNOTIFY = 0x00000004,
            WS_EX_TOPMOST = 0x00000008,
            WS_EX_ACCEPTFILES = 0x00000010,
            WS_EX_TRANSPARENT = 0x00000020,
        }

        [DllImport("kernel32", SetLastError = true)]
        public static extern bool CloseHandle(IntPtr threadHandle);

        [DllImport("kernel32", SetLastError = true)]
        public static extern bool CreateProcess(
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

        [DllImport("dwmapi", SetLastError = true)]
        public static extern void DwmExtendFrameIntoClientArea(IntPtr windowHandle, ref Margins margins);

        [DllImport("user32", SetLastError = true)]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32", SetLastError = true)]
        public static extern int GetWindowLong(IntPtr windowHandle, int index);

        [DllImport("user32", SetLastError = true)]
        public static extern bool GetWindowRect(IntPtr windowHandle, ref Rect rectangle);

        [DllImport("user32", SetLastError = true)]
        public static extern int GetWindowThreadProcessId(IntPtr windowHandle, int processId);

        [DllImport("msvcrt", EntryPoint = "memset", SetLastError = false)]
        public static extern IntPtr MemSet(IntPtr dest, int c, int count);

        [DllImport("user32", SetLastError = true)]
        public static extern bool MoveWindow(IntPtr windowHandle, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("ntdll", SetLastError = true)]
        public static extern bool NtReadVirtualMemory(IntPtr processHandle, IntPtr baseAddress, void* buffer, int size, out IntPtr numberOfBytesRead);

        [DllImport("ntdll", SetLastError = true)]
        public static extern bool NtResumeThread(IntPtr threadHandle, out IntPtr suspendCount);

        [DllImport("ntdll", SetLastError = true)]
        public static extern bool NtSuspendThread(IntPtr threadHandle, out IntPtr previousSuspendCount);

        [DllImport("ntdll", SetLastError = true)]
        public static extern bool NtWriteVirtualMemory(IntPtr processHandle, IntPtr baseAddress, void* buffer, int size, out IntPtr numberOfBytesWritten);

        [DllImport("kernel32", SetLastError = true)]
        public static extern IntPtr OpenProcess(ProcessAccessFlags processAccess, bool inheritHandle, int processId);

        [DllImport("kernel32", SetLastError = true)]
        public static extern IntPtr OpenThread(ThreadAccess threadAccess, bool inheritHandle, uint threadId);

        [DllImport("user32", SetLastError = true)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32", SetLastError = true)]
        public static extern bool SetLayeredWindowAttributes(IntPtr windowHandle, uint colorKey, uint alpha, uint flags);

        [DllImport("user32", SetLastError = true)]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32", SetLastError = true)]
        public static extern int SetWindowLong(IntPtr windowHandle, int index, int newLong);

        [DllImport("user32", SetLastError = true)]
        public static extern int SetWindowLong(IntPtr windowHandle, int nIndex, uint dwNewLong);

        [DllImport("user32", SetLastError = true)]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32", SetLastError = true)]
        public static extern IntPtr SetWindowPos(IntPtr windowHandle, int windowHandleInsertAfter, int x, int y, int cx, int cy, int wFlags);

        [DllImport("user32", SetLastError = true)]
        public static extern bool ShowWindow(IntPtr windowHandle, int nCmdShow);

        [DllImport("kernel32", SetLastError = true)]
        public static extern IntPtr VirtualAllocEx(IntPtr processHandle, IntPtr address, uint size, AllocationType allocationType, MemoryProtection memoryProtection);

        [DllImport("kernel32", SetLastError = true)]
        public static extern bool VirtualFreeEx(IntPtr processHandle, IntPtr address, int size, AllocationType allocationType);

        [DllImport("kernel32", SetLastError = true)]
        public static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, MemoryProtection flNewProtect, out MemoryProtection lpflOldProtect);

        [StructLayout(LayoutKind.Sequential)]
        public struct ProcessInformation
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct StartupInfo
        {
            public int cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public int dwX;
            public int dwY;
            public int dwXSize;
            public int dwYSize;
            public int dwXCountChars;
            public int dwYCountChars;
            public int dwFillAttribute;
            public int dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }
    }
}