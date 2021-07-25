using AmeisenBotX.Memory.Structs;
using AmeisenBotX.Memory.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using static AmeisenBotX.Memory.Win32.Win32Imports;

namespace AmeisenBotX.Memory
{
    public unsafe class XMemory : IMemoryApi
    {
        private const int FASM_MEMORY_SIZE = 8192;
        private const int FASM_PASSES = 100;
        private const int INITIAL_POOL_SIZE = 16384;

        // lock needs to be static as FASM isn't thread safe
        private static readonly object fasmLock = new();

        private readonly object allocLock = new();
        private ulong rpmCalls;
        private ulong wpmCalls;

        public XMemory()
        {
            AssemblyBuffer = new();

            if (!File.Exists("FASM.dll"))
            {
                throw new FileNotFoundException("The mandatory \"FASM.dll\" could not be found on your system, download it from the Flat Assembler forum!");
            }
        }

        ///<inheritdoc cref="IMemoryApi.AssemblyBuffer"/>
        public StringBuilder AssemblyBuffer { get; private set; }

        ///<inheritdoc cref="IMemoryApi.MainThreadHandle"/>
        public IntPtr MainThreadHandle { get; private set; }

        ///<inheritdoc cref="IMemoryApi.MemoryAllocations"/>
        public Dictionary<IntPtr, uint> MemoryAllocations => AllocationPools.ToDictionary(e => e.Address, e => (uint)e.Size);

        ///<inheritdoc cref="IMemoryApi.Process"/>
        public Process Process { get; private set; }

        ///<inheritdoc cref="IMemoryApi.ProcessHandle"/>
        public IntPtr ProcessHandle { get; private set; }

        ///<inheritdoc cref="IMemoryApi.RpmCallCount"/>
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

        ///<inheritdoc cref="IMemoryApi.WpmCallCount"/>
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

        private List<AllocationPool> AllocationPools { get; set; }

        ///<inheritdoc cref="IMemoryApi.AllocateMemory"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AllocateMemory(uint size, out IntPtr address)
        {
            lock (allocLock)
            {
                for (int i = 0; i < AllocationPools.Count; ++i)
                {
                    if (AllocationPools[i].Reserve((int)size, out address))
                    {
                        return true;
                    }
                }

                // we need a new pool
                int newPoolSize = Math.Max((int)size, INITIAL_POOL_SIZE);
                IntPtr newPoolAddress = VirtualAllocEx(ProcessHandle, IntPtr.Zero, (uint)newPoolSize, AllocationTypes.Commit, MemoryProtectionFlags.ExecuteReadWrite);

                if (newPoolAddress != IntPtr.Zero)
                {
                    AllocationPool pool = new(newPoolAddress, newPoolSize);
                    AllocationPools.Add(pool);

                    return pool.Reserve((int)size, out address);
                }

                address = IntPtr.Zero;
                return false;
            }
        }

        ///<inheritdoc cref="IMemoryApi.Dispose"/>
        public void Dispose()
        {
            GC.SuppressFinalize(this);

            CloseHandle(MainThreadHandle);
            CloseHandle(ProcessHandle);

            FreeAllMemory();
        }

        ///<inheritdoc cref="IMemoryApi.FocusWindow"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FocusWindow(IntPtr windowHandle, Rect rect, bool resizeWindow = true)
        {
            WindowFlags flags = WindowFlags.AsyncWindowPos | WindowFlags.NoActivate;

            if (!resizeWindow) { flags |= WindowFlags.NoSize; }

            if (rect.Left > 0 && rect.Right > 0 && rect.Top > 0 && rect.Bottom > 0)
            {
                SetWindowPos(windowHandle, IntPtr.Zero, rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top, (int)flags);
            }
        }

        ///<inheritdoc cref="IMemoryApi.FreeAllMemory"/>
        public void FreeAllMemory()
        {
            lock (allocLock)
            {
                for (int i = 0; i < AllocationPools.Count; ++i)
                {
                    VirtualFreeEx(ProcessHandle, AllocationPools[i].Address, 0, AllocationTypes.Release);
                }

                AllocationPools.Clear();
            }
        }

        ///<inheritdoc cref="IMemoryApi.FreeMemory"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool FreeMemory(IntPtr address)
        {
            lock (allocLock)
            {
                for (int i = 0; i < AllocationPools.Count; ++i)
                {
                    if (AllocationPools[i].Free(address))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        ///<inheritdoc cref="IMemoryApi.GetClientSize"/>
        public Rect GetClientSize()
        {
            Rect rect = new();
            GetClientRect(Process.MainWindowHandle, ref rect);
            return rect;
        }

        ///<inheritdoc cref="IMemoryApi.GetForegroundWindow"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IntPtr GetForegroundWindow()
        {
            return Win32Imports.GetForegroundWindow();
        }

        ///<inheritdoc cref="IMemoryApi.GetWindowPosition"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Rect GetWindowPosition()
        {
            Rect rect = new();
            GetWindowRect(Process.MainWindowHandle, ref rect);
            return rect;
        }

        ///<inheritdoc cref="IMemoryApi.Init"/>
        public bool Init(Process process, IntPtr processHandle, IntPtr mainThreadHandle)
        {
            Process = process;

            if (Process == null || Process.HasExited)
            {
                return false;
            }

            ProcessHandle = processHandle;

            if (ProcessHandle == IntPtr.Zero)
            {
                return false;
            }

            MainThreadHandle = mainThreadHandle;

            if (MainThreadHandle == IntPtr.Zero)
            {
                return false;
            }

            // reserve initial pool
            AllocationPools = new();

            IntPtr initialPoolAddress = VirtualAllocEx(ProcessHandle, IntPtr.Zero, (uint)INITIAL_POOL_SIZE, AllocationTypes.Commit, MemoryProtectionFlags.ExecuteReadWrite);

            if (initialPoolAddress == IntPtr.Zero)
            {
                return false;
            }

            AllocationPools.Add(new(initialPoolAddress, INITIAL_POOL_SIZE));

            return true;
        }

        ///<inheritdoc cref="IMemoryApi.InjectAssembly"/>
        public bool InjectAssembly(IntPtr address, bool patchMemProtection = false)
        {
            lock (fasmLock)
            {
                AssemblyBuffer.Insert(0, $"use32\norg 0x{address:X08}\n");

                fixed (byte* pBytes = stackalloc byte[FASM_MEMORY_SIZE])
                {
                    if (FasmAssemble(AssemblyBuffer.ToString(), pBytes, FASM_MEMORY_SIZE, FASM_PASSES, IntPtr.Zero) == 0)
                    {
                        FasmStateOk state = *(FasmStateOk*)pBytes;

                        if (patchMemProtection)
                        {
                            if (MemoryProtect(address, state.OutputLength, MemoryProtectionFlags.ExecuteReadWrite, out MemoryProtectionFlags oldMemoryProtection))
                            {
                                bool status = !NtWriteVirtualMemory(ProcessHandle, address, (void*)state.OutputData, (int)state.OutputLength, out _);
                                MemoryProtect(address, state.OutputLength, oldMemoryProtection, out _);

                                AssemblyBuffer.Clear();
                                return status;
                            }
                        }
                        else
                        {
                            AssemblyBuffer.Clear();
                            return !NtWriteVirtualMemory(ProcessHandle, address, (void*)state.OutputData, (int)state.OutputLength, out _);
                        }
                    }

                    // use this to read the error
                    FasmStateError stateError = *(FasmStateError*)pBytes;

                    AssemblyBuffer.Clear();
                    return false;
                }
            }
        }

        ///<inheritdoc cref="IMemoryApi.MemoryProtect"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MemoryProtect(IntPtr address, uint size, MemoryProtectionFlags memoryProtection, out MemoryProtectionFlags oldMemoryProtection)
        {
            return VirtualProtectEx(ProcessHandle, address, size, memoryProtection, out oldMemoryProtection);
        }

        ///<inheritdoc cref="IMemoryApi.PatchMemory"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PatchMemory<T>(IntPtr address, T data) where T : unmanaged
        {
            uint size = (uint)sizeof(T);

            if (MemoryProtect(address, size, MemoryProtectionFlags.ExecuteReadWrite, out MemoryProtectionFlags oldMemoryProtection))
            {
                Write(address, data);
                MemoryProtect(address, size, oldMemoryProtection, out _);
            }
        }

        ///<inheritdoc cref="IMemoryApi.Read"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Read<T>(IntPtr address, out T value) where T : unmanaged
        {
            int size = sizeof(T);

            fixed (byte* pBuffer = stackalloc byte[size])
            {
                if (RpmGateWay(address, pBuffer, size))
                {
                    value = *(T*)pBuffer;
                    return true;
                }
            }

            value = default;
            return false;
        }

        ///<inheritdoc cref="IMemoryApi.ReadBytes"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReadBytes(IntPtr address, int size, out byte[] bytes)
        {
            byte[] buffer = new byte[size];

            fixed (byte* pBuffer = buffer)
            {
                if (RpmGateWay(address, pBuffer, size))
                {
                    bytes = buffer;
                    return true;
                }
            }

            bytes = null;
            return false;
        }

        ///<inheritdoc cref="IMemoryApi.ReadString"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReadString(IntPtr address, Encoding encoding, out string value, int bufferSize = 512)
        {
            StringBuilder sb = new();

            fixed (byte* pBuffer = stackalloc byte[bufferSize])
            {
                int i;

                do
                {
                    if (!RpmGateWay(address, pBuffer, bufferSize))
                    {
                        value = string.Empty;
                        return false;
                    }

                    i = 0;

                    while (i < bufferSize && pBuffer[i] != 0) { ++i; }

                    address += i;

                    sb.Append(encoding.GetString(pBuffer, i));
                } while (i == bufferSize);

                value = sb.ToString();
                return true;
            }
        }

        ///<inheritdoc cref="IMemoryApi.ResizeParentWindow"/>
        public void ResizeParentWindow(int offsetX, int offsetY, int width, int height)
        {
            SetWindowPos(Process.MainWindowHandle, IntPtr.Zero, offsetX, offsetY, width, height, SWP_NOZORDER | SWP_NOACTIVATE);
        }

        ///<inheritdoc cref="IMemoryApi.ResumeMainThread"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ResumeMainThread()
        {
            NtResumeThread(MainThreadHandle, out _);
        }

        ///<inheritdoc cref="IMemoryApi.SetForegroundWindow"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetForegroundWindow(IntPtr windowHandle)
        {
            Win32Imports.SetForegroundWindow(windowHandle);
        }

        ///<inheritdoc cref="IMemoryApi.SetupAutoPosition"/>
        public void SetupAutoPosition(IntPtr mainWindowHandle, int offsetX, int offsetY, int width, int height)
        {
            if (Process.MainWindowHandle != IntPtr.Zero && mainWindowHandle != IntPtr.Zero)
            {
                SetParent(Process.MainWindowHandle, mainWindowHandle);

                int style = GetWindowLong(Process.MainWindowHandle, GWL_STYLE) & ~(int)WindowStyles.WS_CAPTION & ~(int)WindowStyles.WS_THICKFRAME;
                SetWindowLong(Process.MainWindowHandle, GWL_STYLE, style);

                ResizeParentWindow(offsetX, offsetY, width, height);
            }
        }

        ///<inheritdoc cref="IMemoryApi.SetWindowPosition"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetWindowPosition(IntPtr windowHandle, Rect rect, bool resizeWindow = true)
        {
            WindowFlags flags = WindowFlags.AsyncWindowPos | WindowFlags.NoZOrder | WindowFlags.NoActivate;

            if (!resizeWindow) { flags |= WindowFlags.NoSize; }

            if (rect.Left > 0 && rect.Right > 0 && rect.Top > 0 && rect.Bottom > 0)
            {
                SetWindowPos(windowHandle, IntPtr.Zero, rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top, (int)flags);
            }
        }

        ///<inheritdoc cref="IMemoryApi.StartProcessNoActivate"/>
        public Process StartProcessNoActivate(string processCmd, out IntPtr processHandle, out IntPtr threadHandle)
        {
            StartupInfo startupInfo = new()
            {
                cb = Marshal.SizeOf<StartupInfo>(),
                dwFlags = STARTF_USESHOWWINDOW,
                wShowWindow = SW_SHOWMINNOACTIVE
            };

            if (CreateProcess(null, processCmd, IntPtr.Zero, IntPtr.Zero, true, 0x10, IntPtr.Zero, null, ref startupInfo, out ProcessInformation processInformation))
            {
                processHandle = processInformation.hProcess;
                threadHandle = processInformation.hThread;
                return Process.GetProcessById(processInformation.dwProcessId);
            }
            else
            {
                processHandle = IntPtr.Zero;
                threadHandle = IntPtr.Zero;
                return null;
            }
        }

        ///<inheritdoc cref="IMemoryApi.SuspendMainThread"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SuspendMainThread()
        {
            NtSuspendThread(MainThreadHandle, out _);
        }

        ///<inheritdoc cref="IMemoryApi.Write"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Write<T>(IntPtr address, T value) where T : unmanaged
        {
            return WpmGateWay(address, &value, sizeof(T));
        }

        ///<inheritdoc cref="IMemoryApi.WriteBytes"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool WriteBytes(IntPtr address, byte[] bytes)
        {
            fixed (byte* pBytes = bytes)
            {
                return WpmGateWay(address, pBytes, bytes.Length);
            }
        }

        ///<inheritdoc cref="IMemoryApi.ZeroMemory"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ZeroMemory(IntPtr address, int size)
        {
            return WriteBytes(address, new byte[size]);
        }

        /// <summary>
        /// FASM assembler library is used to assembly our injection stuff.
        /// </summary>
        /// <param name="szSource">Assembly instructions.</param>
        /// <param name="lpMemory">Output bytes</param>
        /// <param name="nSize">Output buffer size</param>
        /// <param name="nPassesLimit">FASM pass limit</param>
        /// <param name="hDisplayPipe">FASM display pipe</param>
        /// <returns>FASM status struct pointer</returns>
        [DllImport("FASM.dll", EntryPoint = "fasm_Assemble", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        private static extern int FasmAssemble(string szSource, byte* lpMemory, int nSize, int nPassesLimit, IntPtr hDisplayPipe);

        /// <summary>
        /// Get FASM assembler version.
        /// </summary>
        /// <returns>Version</returns>
        [DllImport("FASM.dll", EntryPoint = "fasm_GetVersion", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        private static extern int FasmGetVersion();

        /// <summary>
        /// Gateway function to monitor ReadProcessMemory calls.
        /// </summary>
        /// <param name="baseAddress">Address of the memory to read</param>
        /// <param name="buffer">Output bytes</param>
        /// <param name="size">Size of the memory to read</param>
        /// <returns>True if read was successful, false if not</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool RpmGateWay(IntPtr baseAddress, void* buffer, int size)
        {
            ++rpmCalls;
            return !NtReadVirtualMemory(ProcessHandle, baseAddress, buffer, size, out _);
        }

        /// <summary>
        /// Gateway function to monitor WriteProcessMemory calls.
        /// </summary>
        /// <param name="baseAddress">Address of the memory to write</param>
        /// <param name="buffer">Input bytes</param>
        /// <param name="size">Size of the memory to write</param>
        /// <returns>True if write was successful, false if not</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool WpmGateWay(IntPtr baseAddress, void* buffer, int size)
        {
            ++wpmCalls;
            return !NtWriteVirtualMemory(ProcessHandle, baseAddress, buffer, size, out _);
        }
    }
}