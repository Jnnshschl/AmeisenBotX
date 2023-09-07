using AmeisenBotX.Common.Memory;
using AmeisenBotX.Common.Memory.Enums;
using AmeisenBotX.Logging;
using AmeisenBotX.Memory.Structs;
using AmeisenBotX.Memory.Win32;
using AmeisenBotX.Wow.Offsets;
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
        // FASM configuration, if you encounter fasm error, try to increase the values
        private const int FASM_MEMORY_SIZE = 8192;

        private const int FASM_PASSES = 100;

        // initial memory pool size
        private const int INITIAL_POOL_SIZE = 0; // 16384;

        // lock needs to be static as FASM isn't thread safe
        private static readonly object fasmLock = new();

        private readonly object allocLock = new();
        private ulong rpmCalls;
        private ulong wpmCalls;

        public XMemory()
        {
            if (!File.Exists("FASM.dll"))
            {
                throw new FileNotFoundException("The mandatory \"FASM.dll\" could not be found on your system, download it from the Flat Assembler forum!");
            }
        }

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
#if DEBUG
            if (!Initialized) { throw new InvalidOperationException("call Init() before you do anything with this class"); }
            if (size <= 0) { throw new ArgumentOutOfRangeException(nameof(size), "size must be > 0"); }
#endif
            lock (allocLock)
            {
                for (int i = 0; i < AllocationPools.Count; ++i)
                {
                    if (AllocationPools[i].Reserve((int)size, out address))
                    {
                        AmeisenLogger.I.Log("XMemory", $"Reserved {size} bytes in Pool[{i}] at: 0x{address:X}");
                        return true;
                    }
                }

                // we need a new pool
                int newPoolSize = Math.Max((int)size, INITIAL_POOL_SIZE);
                IntPtr newPoolAddress = VirtualAllocEx(ProcessHandle, IntPtr.Zero, (uint)newPoolSize, AllocationType.Commit, MemoryProtectionFlag.ExecuteReadWrite);

                if (newPoolAddress != IntPtr.Zero)
                {
                    AllocationPool pool = new(newPoolAddress, newPoolSize);
                    AllocationPools.Add(pool);

                    AmeisenLogger.I.Log("XMemory", $"Created new Pool with {newPoolSize} bytes at: 0x{newPoolAddress:X}");

                    if (pool.Reserve((int)size, out address))
                    {
                        AmeisenLogger.I.Log("XMemory", $"Reserved {size} bytes in Pool[{AllocationPools.Count - 1}] at: 0x{address:X}");
                        return true;
                    }
                }

                address = IntPtr.Zero;
                return false;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
        }

        ///<inheritdoc cref="IMemoryApi.Dispose"/>
        public void Dispose(bool managed = false)
        {
            GC.SuppressFinalize(this);
            CloseHandle(MainThreadHandle);
            if (MainThreadHandle != ProcessHandle)
                CloseHandle(ProcessHandle);
            RemoveAutoPosition();
            FreeAllMemory();
        }

        ///<inheritdoc cref="IMemoryApi.FocusWindow"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FocusWindow(IntPtr windowHandle, Rect rect, bool resizeWindow = true)
        {
            WindowFlag flags = WindowFlag.AsyncWindowPos | WindowFlag.NoActivate;

            if (!resizeWindow) { flags |= WindowFlag.NoSize; }

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
                if (AllocationPools != null)
                {
                    AmeisenLogger.I.Log("XMemory", $"Freeing all memory Pools...");

                    foreach (AllocationPool allocPool in AllocationPools)
                    {
                        VirtualFreeEx(ProcessHandle, allocPool.Address, 0, AllocationType.Release);
                    }

                    AllocationPools.Clear();
                }
            }
        }

        ///<inheritdoc cref="IMemoryApi.FreeMemory"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool FreeMemory(IntPtr address)
        {
#if DEBUG
            if (!Initialized) { throw new InvalidOperationException("call Init() before you do anything with this class"); }
            if (address == IntPtr.Zero) { throw new ArgumentOutOfRangeException(nameof(address), "address must be > 0"); }
#endif
            lock (allocLock)
            {
                for (int i = 0; i < AllocationPools.Count; ++i)
                {
                    if (AllocationPools[i].Free(address, out int size)
                        && ZeroMemory(address, size))
                    {
                        AmeisenLogger.I.Log("XMemory", $"Freed {size} bytes in Pool[{i}] at: 0x{address:X}");

                        // if (AllocationPools[i].Allocations.Count == 0 &&
                        // VirtualFreeEx(ProcessHandle, AllocationPools[i].Address, 0,
                        // AllocationType.Release)) { AmeisenLogger.I.Log("XMemory", $"Freed
                        // Pool[{i}] with {AllocationPools[i].Size} bytes at: 0x{address:X}");
                        // AllocationPools.RemoveAt(i); }

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

            if (Process != null)
            {
                GetWindowRect(Process.MainWindowHandle, ref rect);
            }

            return rect;
        }

        private bool Initialized { get; set; }

        public IOffsetList Offsets => throw new NotImplementedException();

        ///<inheritdoc cref="IMemoryApi.Init"/>
        public virtual bool Init(Process process, IntPtr processHandle, IntPtr mainThreadHandle)
        {
            Process = process ?? throw new ArgumentNullException(nameof(process), "process cannot be null");

#if DEBUG
            if (processHandle == IntPtr.Zero) { throw new ArgumentOutOfRangeException(nameof(processHandle), "processHandle must be > 0"); }
            if (mainThreadHandle == IntPtr.Zero) { throw new ArgumentOutOfRangeException(nameof(mainThreadHandle), "mainThreadHandle must be > 0"); }
#endif
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

            AllocationPools = new();

            // reserve initial pool
            if (INITIAL_POOL_SIZE > 0)
            {
                IntPtr initialPoolAddress = VirtualAllocEx(ProcessHandle, IntPtr.Zero, INITIAL_POOL_SIZE, AllocationType.Commit, MemoryProtectionFlag.ExecuteReadWrite);

                if (initialPoolAddress == IntPtr.Zero)
                {
                    return false;
                }

                AllocationPools.Add(new(initialPoolAddress, INITIAL_POOL_SIZE));
            }

            Initialized = true;
            return true;
        }

        ///<inheritdoc cref="IMemoryApi.InjectAssembly"/>
        public bool InjectAssembly(IEnumerable<string> asm, IntPtr address, bool patchMemProtection = false)
        {
#if DEBUG
            if (!Initialized) { throw new InvalidOperationException("call Init() before you do anything with this class"); }
            if (!asm.Any()) { throw new ArgumentOutOfRangeException(nameof(asm), "asm must contain atleast one instruction"); }
            if (address == IntPtr.Zero) { throw new ArgumentOutOfRangeException(nameof(address), "address must be > 0"); }
#endif
            lock (fasmLock)
            {
                fixed (byte* pBytes = stackalloc byte[FASM_MEMORY_SIZE])
                {
                    if (FasmAssemble($"use32\norg 0x{address:X08}\n{string.Join('\n', asm)}", pBytes, FASM_MEMORY_SIZE, FASM_PASSES, IntPtr.Zero) == 0)
                    {
                        FasmStateOk state = *(FasmStateOk*)pBytes;

                        if (patchMemProtection)
                        {
                            if (ProtectMemory(address, state.OutputLength, MemoryProtectionFlag.ExecuteReadWrite, out MemoryProtectionFlag oldMemoryProtection))
                            {
                                bool status = !NtWriteVirtualMemory(ProcessHandle, address, (void*)state.OutputData, (int)state.OutputLength, out _);
                                ProtectMemory(address, state.OutputLength, oldMemoryProtection, out _);
                                return status;
                            }
                        }
                        else
                        {
                            return !NtWriteVirtualMemory(ProcessHandle, address, (void*)state.OutputData, (int)state.OutputLength, out _);
                        }
                    }

                    // use this to read the error FasmStateError stateError = *(FasmStateError*)pBytes;
                    return false;
                }
            }
        }

        ///<inheritdoc cref="IMemoryApi.ProtectMemory"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ProtectMemory(IntPtr address, uint size, MemoryProtectionFlag memoryProtection, out MemoryProtectionFlag oldMemoryProtection)
        {
#if DEBUG
            if (!Initialized) { throw new InvalidOperationException("call Init() before you do anything with this class"); }
            if (address == IntPtr.Zero) { throw new ArgumentOutOfRangeException(nameof(address), "address must be > 0"); }
            if (size <= 0) { throw new ArgumentOutOfRangeException(nameof(size), "size must be > 0"); }
#endif
            return VirtualProtectEx(ProcessHandle, address, size, memoryProtection, out oldMemoryProtection);
        }

        ///<inheritdoc cref="IMemoryApi.PatchMemory"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PatchMemory<T>(IntPtr address, T data) where T : unmanaged
        {
#if DEBUG
            if (!Initialized) { throw new InvalidOperationException("call Init() before you do anything with this class"); }
            if (address == IntPtr.Zero) { throw new ArgumentOutOfRangeException(nameof(address), "address must be > 0"); }
#endif
            uint size = (uint)sizeof(T);

            if (ProtectMemory(address, size, MemoryProtectionFlag.ExecuteReadWrite, out MemoryProtectionFlag oldMemoryProtection))
            {
                Write(address, data);
                ProtectMemory(address, size, oldMemoryProtection, out _);
            }
        }

        ///<inheritdoc cref="IMemoryApi.Read"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Read<T>(IntPtr address, out T value) where T : unmanaged
        {
#if DEBUG
            if (!Initialized) { throw new InvalidOperationException("call Init() before you do anything with this class"); }
            //if (address == IntPtr.Zero) { throw new ArgumentOutOfRangeException(nameof(address), "address must be > 0"); }
#endif
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
#if DEBUG
            if (!Initialized) { throw new InvalidOperationException("call Init() before you do anything with this class"); }
            if (address == IntPtr.Zero) { throw new ArgumentOutOfRangeException(nameof(address), "address must be > 0"); }
            if (size <= 0) { throw new ArgumentOutOfRangeException(nameof(size), "size must be > 0"); }
#endif
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
#if DEBUG
            if (!Initialized) { throw new InvalidOperationException("call Init() before you do anything with this class"); }
            if (address == IntPtr.Zero) { throw new ArgumentOutOfRangeException(nameof(address), "address must be > 0"); }
            if (encoding == null) { throw new ArgumentNullException(nameof(encoding), "encoding cannot be null"); }
            if (bufferSize <= 0) { throw new ArgumentOutOfRangeException(nameof(bufferSize), "bufferSize must be > 0"); }
#endif
            StringBuilder sb = new(bufferSize);

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

                    while (i < bufferSize && pBuffer[i] != 0)
                    {
                        ++i;
                    }

                    address += i;

                    sb.Append(encoding.GetString(pBuffer, i));
                } while (i == bufferSize);

                value = sb.ToString();
                return true;
            }
        }

        ///<inheritdoc cref="IMemoryApi.ResizeParentWindow"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ResizeParentWindow(int offsetX, int offsetY, int width, int height)
        {
#if DEBUG
            if (!Initialized) { throw new InvalidOperationException("call Init() before you do anything with this class"); }
#endif
            SetWindowPos(Process.MainWindowHandle, IntPtr.Zero, offsetX, offsetY, width, height, SWP_NOZORDER | SWP_NOACTIVATE);
        }

        ///<inheritdoc cref="IMemoryApi.ResumeMainThread"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ResumeMainThread()
        {
#if DEBUG
            if (!Initialized) { throw new InvalidOperationException("call Init() before you do anything with this class"); }
#endif
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
#if DEBUG
            if (!Initialized) { throw new InvalidOperationException("call Init() before you do anything with this class"); }
#endif
            if (Process.MainWindowHandle != IntPtr.Zero && mainWindowHandle != IntPtr.Zero)
            {
                SetParent(Process.MainWindowHandle, mainWindowHandle);

                int style = GetWindowLong(Process.MainWindowHandle, GWL_STYLE) & ~(int)WindowStyle.WS_CAPTION & ~(int)WindowStyle.WS_THICKFRAME;
                SetWindowLong(Process.MainWindowHandle, GWL_STYLE, style);

                ResizeParentWindow(offsetX, offsetY, width, height);
            }
        }

        public void RemoveAutoPosition()
        {
            if (Process == null)
                return;

            Process?.Refresh();

            if (Process?.MainWindowHandle != IntPtr.Zero)
            {
                SetParent(Process.MainWindowHandle, IntPtr.Zero);
                uint style = (uint)GetWindowLong(Process.MainWindowHandle, GWL_STYLE);
                style = (style
                        | (uint)WindowStyle.WS_POPUP
                        | (uint)WindowStyle.WS_THICKFRAME
                        | (uint)WindowStyle.WS_CAPTION
                    ) & (~(uint)WindowStyle.WS_CHILD);
                SetWindowLong(Process.MainWindowHandle, GWL_STYLE, (int)style);
            }
        }

        ///<inheritdoc cref="IMemoryApi.SetWindowPosition"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetWindowPosition(IntPtr windowHandle, Rect rect, bool resizeWindow = true)
        {
            WindowFlag flags = WindowFlag.AsyncWindowPos | WindowFlag.NoZOrder | WindowFlag.NoActivate;

            if (!resizeWindow) { flags |= WindowFlag.NoSize; }

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

        ///<inheritdoc/>
        public Process AttachProcess(int processId, out IntPtr processHandle, out IntPtr threadHandle)
        {
            Process process = Process.GetProcessById(processId);

            processHandle = OpenProcess(ProcessAccessFlag.All, false, processId);
            threadHandle = GetMainThreadHandle(processId);

            return process;

            static IntPtr GetMainThreadHandle(int processId)
            {
                Process process = Process.GetProcessById(processId);
                ProcessThread primaryThread = process.Threads.Cast<ProcessThread>().OrderBy(t => t.StartTime).FirstOrDefault();

                if (primaryThread != null)
                {
                    return OpenThread(ThreadAccessFlag.SuspendResume, false, (uint)primaryThread.Id);
                }

                return IntPtr.Zero;
            }
        }

        ///<inheritdoc cref="IMemoryApi.SuspendMainThread"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SuspendMainThread()
        {
#if DEBUG
            if (!Initialized) { throw new InvalidOperationException("call Init() before you do anything with this class"); }
#endif
            NtSuspendThread(MainThreadHandle, out _);
        }

        ///<inheritdoc cref="IMemoryApi.Write"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Write<T>(IntPtr address, T value) where T : unmanaged
        {
#if DEBUG
            if (!Initialized) { throw new InvalidOperationException("call Init() before you do anything with this class"); }
            if (address == IntPtr.Zero) { throw new ArgumentOutOfRangeException(nameof(address), "address must be > 0"); }
#endif
            return WpmGateWay(address, &value, sizeof(T));
        }

        ///<inheritdoc cref="IMemoryApi.WriteBytes"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool WriteBytes(IntPtr address, byte[] bytes)
        {
#if DEBUG
            if (!Initialized) { throw new InvalidOperationException("call Init() before you do anything with this class"); }
            if (address == IntPtr.Zero) { throw new ArgumentOutOfRangeException(nameof(address), "address must be > 0"); }
            if (bytes?.Length <= 0) { throw new ArgumentOutOfRangeException(nameof(bytes), "bytes size must be > 0"); }
#endif
            fixed (byte* pBytes = bytes)
            {
                return WpmGateWay(address, pBytes, bytes.Length);
            }
        }

        ///<inheritdoc cref="IMemoryApi.ZeroMemory"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ZeroMemory(IntPtr address, int size)
        {
#if DEBUG
            if (!Initialized) { throw new InvalidOperationException("call Init() before you do anything with this class"); }
            if (address == IntPtr.Zero) { throw new ArgumentOutOfRangeException(nameof(address), "address must be > 0"); }
            if (size <= 0) { throw new ArgumentOutOfRangeException(nameof(size), "size must be > 0"); }
#endif
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
            var ret = !NtReadVirtualMemory(ProcessHandle, baseAddress, buffer, size, out IntPtr bytesRead);
            return ret;
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