using AmeisenBotX.Memory.Win32;
using Fasm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
            foreach (IntPtr memAlloc in MemoryAllocations.Keys)
            {
                FreeMemory(memAlloc);
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

        public ProcessThread GetMainThread()
        {
            if (Process.MainWindowHandle == null) { return null; }

            int id = GetWindowThreadProcessId(Process.MainWindowHandle, 0);
            foreach (ProcessThread processThread in Process.Threads)
            {
                if (processThread.Id == id)
                {
                    return processThread;
                }
            }

            return null;
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
            MainThreadHandle = OpenThread(ThreadAccess.SuspendResume, false, (uint)GetMainThread().Id);
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