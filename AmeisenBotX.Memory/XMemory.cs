using AmeisenBotX.Memory.Win32;
using Fasm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using static AmeisenBotX.Memory.Win32.Win32Imports;

namespace AmeisenBotX.Memory
{
    public class XMemory
    {
        private ulong rpmCalls;
        private ulong wpmCalls;

        public XMemory()
        {
            SizeCache = new Dictionary<Type, int>();
            MemoryAllocations = new Dictionary<IntPtr, uint>();
        }

        ~XMemory()
        {
            CloseHandle(MainThreadHandle);

            List<IntPtr> memAllocs = MemoryAllocations.Keys.ToList();

            for (int i = 0; i < memAllocs.Count; ++i)
            {
                FreeMemory(memAllocs[i]);
            }
        }

        public int AllocationCount
            => MemoryAllocations.Count;

        public ManagedFasm Fasm { get; private set; }

        public IntPtr MainThreadHandle { get; private set; }

        public Dictionary<IntPtr, uint> MemoryAllocations { get; }

        public Process Process { get; private set; }

        public IntPtr ProcessHandle { get; private set; }

        public ulong RpmCallCount
        {
            get
            {
                unchecked
                {
                    ulong val = rpmCalls;
                    rpmCalls = 0;
                    return val;
                }
            }
        }

        public ulong WpmCallCount
        {
            get
            {
                unchecked
                {
                    ulong val = wpmCalls;
                    wpmCalls = 0;
                    return val;
                }
            }
        }

        private Dictionary<Type, int> SizeCache { get; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BringWindowToFront(IntPtr windowHandle, Rect rect, bool resizeWindow = true)
        {
            WindowFlags flags = WindowFlags.AsyncWindowPos | WindowFlags.NoActivate;
            if (!resizeWindow) { flags |= WindowFlags.NoSize; }

            if (rect.Left > 0 && rect.Right > 0 && rect.Top > 0 && rect.Bottom > 0)
            {
                SetWindowPos(windowHandle, 0, rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top, (int)flags);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rect GetWindowPosition(IntPtr windowHandle)
        {
            Rect rect = new Rect();
            GetWindowRect(windowHandle, ref rect);
            return rect;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetWindowPosition(IntPtr windowHandle, Rect rect, bool resizeWindow = true)
        {
            WindowFlags flags = WindowFlags.AsyncWindowPos | WindowFlags.NoZOrder | WindowFlags.NoActivate;
            if (!resizeWindow) { flags |= WindowFlags.NoSize; }

            if (rect.Left > 0 && rect.Right > 0 && rect.Top > 0 && rect.Bottom > 0)
            {
                SetWindowPos(windowHandle, 0, rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top, (int)flags);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AllocateMemory(uint size, out IntPtr address)
        {
            address = VirtualAllocEx(ProcessHandle, IntPtr.Zero, size, AllocationType.Commit, MemoryProtection.ExecuteReadWrite);
            if (address != IntPtr.Zero)
            {
                MemoryAllocations.Add(address, size);
                return true;
            }

            address = IntPtr.Zero;
            return false;
        }

        public bool Attach(Process wowProcess)
        {
            Process = wowProcess;
            if (Process == null || Process.HasExited)
            {
                return false;
            }

            ProcessHandle = OpenProcess(ProcessAccessFlags.All, false, wowProcess.Id);
            if (ProcessHandle == IntPtr.Zero)
            {
                return false;
            }

            Fasm = new ManagedFasm(ProcessHandle);
            MemoryAllocations.Clear();
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool FreeMemory(IntPtr address)
        {
            if (MemoryAllocations.ContainsKey(address))
            {
                MemoryAllocations.Remove(address);
                return VirtualFreeEx(ProcessHandle, address, 0, AllocationType.Release);
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IntPtr GetForegroundWindow()
        {
            return Win32Imports.GetForegroundWindow();
        }

        public ProcessThread GetMainThread()
        {
            if (Process.MainWindowHandle == null) { return null; }

            int id = GetWindowThreadProcessId(Process.MainWindowHandle, 0);

            for(int i = 0; i < Process.Threads.Count; ++i)
            {
                if (Process.Threads[i].Id == id)
                {
                    return Process.Threads[i];
                }
            }

            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Rect GetWindowPositionWow()
        {
            Rect rect = new Rect();
            GetWindowRect(Process.MainWindowHandle, ref rect);
            return rect;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void HideBordersWindowWow()
        {
            uint currentLong = (uint)GetWindowLong(Process.MainWindowHandle, GWL_STYLE);
            uint flagsToRemove = (int)(WindowStyles.WS_BORDER | WindowStyles.WS_CAPTION | WindowStyles.WS_THICKFRAME | WindowStyles.WS_MINIMIZE | WindowStyles.WS_MAXIMIZE | WindowStyles.WS_SYSMENU);
            uint newLong = currentLong & ~flagsToRemove;

            Win32Imports.SetWindowLong(Process.MainWindowHandle, GWL_STYLE, newLong);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MemoryProtect(IntPtr address, uint size, MemoryProtection memoryProtection, out MemoryProtection oldMemoryProtection)
        {
            return VirtualProtectEx(ProcessHandle, address, size, memoryProtection, out oldMemoryProtection);
        }

        public void MoveWindowWow(int x, int y, int windowHandle, int height, bool repaint)
        {
            Win32Imports.MoveWindow(Process.MainWindowHandle, x, y, windowHandle, height, repaint);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PatchMemory<T>(IntPtr address, T data) where T : unmanaged
        {
            uint size = (uint)SizeOf<T>();

            if (MemoryProtect(address, size, MemoryProtection.ExecuteReadWrite, out MemoryProtection oldMemoryProtection))
            {
                Write(address, data);
                MemoryProtect(address, size, oldMemoryProtection, out _);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe bool Read<T>(IntPtr address, out T value) where T : unmanaged
        {
            int size = SizeOf<T>();
            byte[] buffer = new byte[size];

            if (RpmGateWay(address, buffer, size))
            {
                fixed (byte* ptr = buffer)
                {
                    value = *(T*)ptr;
                    return true;
                }
            }

            value = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReadBytes(IntPtr address, int size, out byte[] bytes)
        {
            byte[] buffer = new byte[size];

            if (RpmGateWay(address, buffer, size))
            {
                bytes = buffer;
                return true;
            }

            bytes = new byte[size];
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReadString(IntPtr address, Encoding encoding, out string value, int lenght = 128)
        {
            byte[] buffer = new byte[lenght];

            if (RpmGateWay(address, buffer, lenght))
            {
                List<byte> strBuffer = new List<byte>();

                for (int i = 0; i < lenght; ++i)
                {
                    if (buffer[i] == 0) { break; }
                    strBuffer.Add(buffer[i]);
                }

                value = encoding.GetString(strBuffer.ToArray());
                return true;
            }

            value = string.Empty;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe bool ReadStruct<T>(IntPtr address, out T value) where T : struct
        {
            int size = SizeOf<T>();
            byte[] buffer = new byte[size];

            if (RpmGateWay(address, buffer, size))
            {
                fixed (byte* pBuffer = buffer)
                {
                    value = Unsafe.Read<T>(pBuffer);
                }

                return true;
            }

            value = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ResumeMainThread()
        {
            if (OpenMainThread())
            {
                NtResumeThread(MainThreadHandle, out _);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetForegroundWindow(IntPtr windowHandle)
        {
            Win32Imports.SetForegroundWindow(windowHandle);
        }

        public void SetWindowParent(IntPtr childHandle, IntPtr parentHandle)
        {
            HideBordersWindowWow();
            Win32Imports.SetWindowLong(childHandle, GWL_STYLE, GetWindowLong(childHandle, GWL_STYLE) | WS_CHILD);

            SetParent(childHandle, parentHandle);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetWindowPositionWow(Rect rect, bool resizeWindow = true)
        {
            WindowFlags flags = WindowFlags.AsyncWindowPos | WindowFlags.NoZOrder | WindowFlags.NoActivate;
            if (!resizeWindow) { flags |= WindowFlags.NoSize; }

            if (rect.Left > 0 && rect.Right > 0 && rect.Top > 0 && rect.Bottom > 0)
            {
                SetWindowPos(Process.MainWindowHandle, 0, rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top, (int)flags);
            }
        }

        public void SetWowWindowOwner(IntPtr owner)
        {
            Win32Imports.SetWindowLong(Process.MainWindowHandle, -8, owner);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ShowWindow(IntPtr windowHandle)
        {
            Win32Imports.ShowWindow(windowHandle, 0x5);
        }

        public Process StartProcessNoActivate(string processCmd)
        {
            StartupInfo startupInfo = new StartupInfo
            {
                cb = SizeOf<StartupInfo>(),
                dwFlags = STARTF_USESHOWWINDOW,
                wShowWindow = SW_SHOWMINNOACTIVE
            };

            if (CreateProcess(null, processCmd, IntPtr.Zero, IntPtr.Zero, true, 0x10, IntPtr.Zero, null, ref startupInfo, out ProcessInformation processInformation))
            {
                CloseHandle(processInformation.hProcess);
                CloseHandle(processInformation.hThread);

                return Process.GetProcessById(processInformation.dwProcessId);
            }
            else
            {
                return null;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SuspendMainThread()
        {
            if (OpenMainThread())
            {
                NtSuspendThread(MainThreadHandle, out _);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe bool Write<T>(IntPtr address, T value) where T : struct
        {
            int size = SizeOf<T>();
            byte[] buffer = new byte[size];

            fixed (byte* pBuffer = buffer)
            {
                Unsafe.Write(pBuffer, value);
            }

            return WpmGateWay(address, buffer, size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool WriteBytes(IntPtr address, byte[] bytes)
        {
            return WpmGateWay(address, bytes, bytes.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ZeroMemory(IntPtr address, int size)
        {
            return WriteBytes(address, new byte[size]);
        }

        private bool OpenMainThread()
        {
            try
            {
                MainThreadHandle = OpenThread(ThreadAccess.SuspendResume, false, (uint)GetMainThread().Id);
            }
            catch { }

            return MainThreadHandle != IntPtr.Zero;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool RpmGateWay(IntPtr baseAddress, byte[] buffer, int size)
        {
            ++rpmCalls;
            return !NtReadVirtualMemory(ProcessHandle, baseAddress, buffer, size, out _);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int SizeOf<T>()
        {
            if (!SizeCache.ContainsKey(typeof(T)))
            {
                SizeCache.Add(typeof(T), Unsafe.SizeOf<T>());
            }

            return SizeCache[typeof(T)];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool WpmGateWay(IntPtr baseAddress, byte[] buffer, int size)
        {
            ++wpmCalls;
            return !NtWriteVirtualMemory(ProcessHandle, baseAddress, buffer, size, out _);
        }
    }
}