using System;
using System.Runtime.InteropServices;

namespace AmeisenBotX.Memory.Win32
{
    internal class Win32Imports
    {
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
        public enum MemoryProtection
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
        public enum ThreadAccess : int
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
        public enum WindowFlags : int
        {
            NoSize = 0x1,
            NoMove = 0x2,
            NoZOrder = 0x4,
            NoRedraw = 0x8,
            NoActivate = 0x10,
            DrawFrame = 0x20,
            FrameChanged = 0x20,
            SHowWindow = 0x40,
            HideWindow = 0x80,
            NoCopyBits = 0x100,
            NoOwnerZOrder = 0x200,
            NoReposition = 0x200,
            NoSendChanging = 0x400,
            Defererase = 0x2000,
            AsyncWindowPos = 0x4000
        }

        [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool CloseHandle(IntPtr threadHandle);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr windowHandle, ref Rect rectangle);

        [DllImport("user32.dll")]
        public static extern int GetWindowThreadProcessId(IntPtr windowHandle, int processId);

        [DllImport("msvcrt.dll", EntryPoint = "memset", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static extern IntPtr MemSet(IntPtr dest, int c, int count);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool MoveWindow(IntPtr windowHandle, int x, int y, int width, int height, bool repaint);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(ProcessAccessFlags processAccess, bool inheritHandle, int processId);

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenThread(ThreadAccess threadAccess, bool inheritHandle, uint threadId);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(IntPtr processHandle, IntPtr baseAddress, IntPtr buffer, int size, out IntPtr numberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(IntPtr processHandle, IntPtr baseAddress, [Out] byte[] buffer, int size, out IntPtr numberOfBytesRead);

        [DllImport("kernel32.dll")]
        public static extern int ResumeThread(IntPtr threadHandle);

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        public static extern IntPtr SetWindowPos(IntPtr windowHandle, int windowHandleInsertAfter, int x, int y, int cx, int cy, int wFlags);

        [DllImport("kernel32.dll")]
        public static extern uint SuspendThread(IntPtr threadHandle);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern IntPtr VirtualAllocEx(IntPtr processHandle, IntPtr address, uint size, AllocationType allocationType, MemoryProtection memoryProtection);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern bool VirtualFreeEx(IntPtr processHandle, IntPtr address, int size, AllocationType allocationType);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(IntPtr processHandle, IntPtr baseAddress, IntPtr buffer, int size, out IntPtr numberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(IntPtr processHandle, IntPtr baseAddress, byte[] buffer, int size, out IntPtr numberOfBytesWritten);
    }
}