using System;
using System.Runtime.InteropServices;

namespace AmeisenBotX.Memory.Win32
{
    public static unsafe partial class Win32Imports
    {
        public const int GWL_STYLE = -16;
        public const int STARTF_USESHOWWINDOW = 1;
        public const int SW_SHOWMINNOACTIVE = 7;
        public const int SWP_NOACTIVATE = 0x10;
        public const int SWP_NOZORDER = 0x4;

        [Flags]
        public enum AllocationType
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
        public enum MemoryProtectionFlag
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
        public enum ProcessAccessFlag
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
        public enum ThreadAccessFlag
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
        public enum WindowFlag
        {
            NoSize = 0x1,
            NoMove = 0x2,
            NoZOrder = 0x4,
            NoRedraw = 0x8,
            NoActivate = 0x10,
            DrawFrame = 0x20,
            ShowWindow = 0x40,
            HideWindow = 0x80,
            NoCopyBits = 0x100,
            NoOwnerZOrder = 0x200,
            NoSendChanging = 0x400,
            Defererase = 0x2000,
            AsyncWindowPos = 0x4000
        }

        [Flags]
        public enum WindowStyle : uint
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

            WS_MINIMIZEBOX = WS_GROUP,
            WS_MAXIMIZEBOX = WS_TABSTOP,

            WS_CAPTION = WS_BORDER | WS_DLGFRAME,
            WS_TILED = WS_OVERLAPPED,
            WS_ICONIC = WS_MINIMIZE,
            WS_SIZEBOX = WS_THICKFRAME,
            WS_TILEDWINDOW = WS_OVERLAPPEDWINDOW,

#pragma warning disable CA1069
            WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,
#pragma warning restore CA1069
            WS_POPUPWINDOW = WS_POPUP | WS_BORDER | WS_SYSMENU,
            WS_CHILDWINDOW = WS_CHILD,

            WS_EX_DLGMODALFRAME = 0x00000001,
            WS_EX_NOPARENTNOTIFY = 0x00000004,
            WS_EX_TOPMOST = 0x00000008,
            WS_EX_ACCEPTFILES = 0x00000010,
            WS_EX_TRANSPARENT = 0x00000020,
        }

        [LibraryImport("user32")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool GetWindowRect(nint windowHandle, ref Rect rectangle);

        [LibraryImport("ntdll")]
        public static partial int NtOpenProcess
        (
            out nint ProcessHandle,
            int DesiredAccess,
            ref ObjectAttributes ObjectAttributes,
            ref ClientId ClientId
        );

        [LibraryImport("kernel32", EntryPoint = "CreateProcessA")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool CreateProcess
        (
            [MarshalAs(UnmanagedType.LPStr)] string lpApplicationName,
            [MarshalAs(UnmanagedType.LPStr)] string lpCommandLine,
            nint lpProcessAttributes,
            nint lpThreadAttributes,
            [MarshalAs(UnmanagedType.Bool)] bool bInheritHandles,
            uint dwCreationFlags,
            nint lpEnvironment,
            [MarshalAs(UnmanagedType.LPStr)] string lpCurrentDirectory,
            ref StartupInfo lpStartupInfo,
            out ProcessInformation lpProcessInformation
        );

        [LibraryImport("user32")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool GetClientRect(nint windowHandle, ref Rect rectangle);

        [LibraryImport("user32")]
        internal static partial nint GetForegroundWindow();

        [LibraryImport("user32")]
        internal static partial nint GetParent(nint hWnd);

        [LibraryImport("user32", EntryPoint = "GetWindowLongA")]
        internal static partial int GetWindowLong(nint windowHandle, int index);

        [LibraryImport("ntdll")]
        internal static partial int NtAllocateVirtualMemory
        (
            nint processHandle,
            ref nint address,
            int zeroBits,
            ref nint size,
            AllocationType allocationType,
            MemoryProtectionFlag protection
        );

        [LibraryImport("ntdll")]
        internal static partial int NtClose(nint Handle);

        [LibraryImport("ntdll")]
        internal static partial int NtFreeVirtualMemory
        (
            nint processHandle,
            ref nint address,
            ref nint size,
            AllocationType allocationType
        );

        [LibraryImport("ntdll")]
        internal static partial int NtProtectVirtualMemory
        (
            nint processHandle,
            ref nint address,
            ref nint size,
            MemoryProtectionFlag newProtection,
            out MemoryProtectionFlag oldProtection
        );

        [LibraryImport("ntdll")]
        internal static partial int NtReadVirtualMemory
        (
            nint processHandle,
            nint baseAddress,
            void* buffer,
            int size,
            out nint numberOfBytesRead
        );

        [LibraryImport("ntdll")]
        internal static partial int NtResumeThread(nint threadHandle, out nint suspendCount);

        [LibraryImport("ntdll")]
        internal static partial int NtSuspendThread(nint threadHandle, out nint previousSuspendCount);

        [LibraryImport("ntdll")]
        internal static partial int NtWriteVirtualMemory
        (
            nint processHandle,
            nint baseAddress,
            void* buffer,
            int size,
            out nint numberOfBytesWritten
        );

        [LibraryImport("user32")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool SetForegroundWindow(nint hWnd);

        [LibraryImport("user32")]
        internal static partial nint SetParent(nint hWndChild, nint hWndNewParent);

        [LibraryImport("user32", EntryPoint = "SetWindowLongA")]
        internal static partial int SetWindowLong(nint windowHandle, int index, int newLong);

        [LibraryImport("user32")]
        internal static partial nint SetWindowPos
        (
            nint windowHandle,
            nint windowHandleInsertAfter,
            int x,
            int y,
            int cx,
            int cy,
            int wFlags
        );

        [StructLayout(LayoutKind.Sequential)]
        public struct ClientId
        {
            public nint UniqueProcess;
            public nint UniqueThread;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ObjectAttributes
        {
            public int Length;
            public nint RootDirectory;
            public nint ObjectName;
            public uint Attributes;
            public nint SecurityDescriptor;
            public nint SecurityQualityOfService;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ProcessInformation
        {
            public nint hProcess;
            public nint hThread;
            public int dwProcessId;
            public int dwThreadId;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct StartupInfo
        {
            public int cb;
            public char* lpReserved;
            public char* lpDesktop;
            public char* lpTitle;
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
            public nint lpReserved2;
            public nint hStdInput;
            public nint hStdOutput;
            public nint hStdError;
        }
    }
}