using AmeisenBotX.Memory.Structs;
using AmeisenBotX.Memory.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using static AmeisenBotX.Memory.Win32.Win32Imports;

namespace AmeisenBotX.Memory
{
    public unsafe class XMemory : IDisposable
    {
        private const int FASM_MEMORY_SIZE = 8192;
        private const int FASM_PASSES = 100;

        // lock needs to be static as FASM isn't thread safe
        private static readonly object fasmLock = new();

        private readonly object allocLock = new();
        private ulong rpmCalls;
        private ulong wpmCalls;

        public XMemory()
        {
            MemoryAllocations = new();
            Fasm = new();
        }

        public StringBuilder Fasm { get; private set; }

        public IntPtr MainThreadHandle { get; private set; }

        public Dictionary<IntPtr, uint> MemoryAllocations { get; }

        public Process Process { get; private set; }

        public IntPtr ProcessHandle { get; private set; }

        /// <summary>
        /// Get the amount of ReadProcessMemory calls since last time.
        /// </summary>
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

        /// <summary>
        /// Get the amount of WriteProcessMemory calls since last time.
        /// </summary>
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BringWindowToFront(IntPtr windowHandle, Rect rect, bool resizeWindow = true)
        {
            WindowFlags flags = WindowFlags.AsyncWindowPos | WindowFlags.NoActivate;

            if (!resizeWindow) { flags |= WindowFlags.NoSize; }

            if (rect.Left > 0 && rect.Right > 0 && rect.Top > 0 && rect.Bottom > 0)
            {
                SetWindowPos(windowHandle, IntPtr.Zero, rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top, (int)flags);
            }
        }

        /// <summary>
        /// Returns the client window rect of the given window.
        /// </summary>
        /// <param name="windowHandle">Window handle</param>
        /// <returns>The size of the client area of the window.</returns>
        public static Rect GetClientSize(IntPtr windowHandle)
        {
            Rect rect = new();
            GetClientRect(windowHandle, ref rect);
            return rect;
        }

        /// <summary>
        /// Get the current focused window handle.
        /// </summary>
        /// <returns>Window handle</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntPtr GetForegroundWindow()
        {
            return Win32Imports.GetForegroundWindow();
        }

        /// <summary>
        /// Returns the position of the supplied window.
        /// </summary>
        /// <param name="windowHandle">Window handle</param>
        /// <returns>Window position</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rect GetWindowPosition(IntPtr windowHandle)
        {
            Rect rect = new();
            GetWindowRect(windowHandle, ref rect);
            return rect;
        }

        /// <summary>
        /// Focus the specified window.
        /// </summary>
        /// <param name="windowHandle">Window handle</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetForegroundWindow(IntPtr windowHandle)
        {
            Win32Imports.SetForegroundWindow(windowHandle);
        }

        /// <summary>
        /// Set the position of a window.
        /// </summary>
        /// <param name="windowHandle">Window handle</param>
        /// <param name="rect">Position</param>
        /// <param name="resizeWindow">Should we resize the window?</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetWindowPosition(IntPtr windowHandle, Rect rect, bool resizeWindow = true)
        {
            WindowFlags flags = WindowFlags.AsyncWindowPos | WindowFlags.NoZOrder | WindowFlags.NoActivate;

            if (!resizeWindow) { flags |= WindowFlags.NoSize; }

            if (rect.Left > 0 && rect.Right > 0 && rect.Top > 0 && rect.Bottom > 0)
            {
                SetWindowPos(windowHandle, IntPtr.Zero, rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top, (int)flags);
            }
        }

        /// <summary>
        /// Start a process but dont focus its window.
        /// </summary>
        /// <param name="processCmd">Command to run</param>
        /// <param name="processHandle">The native process handle of the started process</param>
        /// <param name="threadHandle">The native thread handle of the started process</param>
        /// <returns>Process object</returns>
        public static Process StartProcessNoActivate(string processCmd, out IntPtr processHandle, out IntPtr threadHandle)
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

        /// <summary>
        /// Allocate memory in the process.
        /// </summary>
        /// <param name="size">Allocation size in bytes</param>
        /// <param name="address">Address of the allocation</param>
        /// <returns>True if allocation was successful, false if not</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AllocateMemory(uint size, out IntPtr address)
        {
            lock (allocLock)
            {
                address = VirtualAllocEx(ProcessHandle, IntPtr.Zero, size, AllocationTypes.Commit, MemoryProtectionFlags.ExecuteReadWrite);

                if (address != IntPtr.Zero)
                {
                    MemoryAllocations.Add(address, size);
                    return true;
                }

                address = IntPtr.Zero;
                return false;
            }
        }

        /// <summary>
        /// Disposes XMemory and frees all memory allocated by it.
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);

            CloseHandle(MainThreadHandle);
            CloseHandle(ProcessHandle);

            FreeAllMemory();
        }

        /// <summary>
        /// Injects the current Fasm buffer and clears it.
        /// </summary>
        /// <param name="address">Address where the fasm will be injected</param>
        /// <param name="patchMemProtection">Whether we need to patch memory protection or nor. Useful when memory is write protected.</param>
        /// <returns>True when the injection was successful</returns>
        public bool FasmInject(IntPtr address, bool patchMemProtection = false)
        {
            lock (fasmLock)
            {
                Fasm.Insert(0, $"org 0x{address:X08}\n");
                Fasm.Insert(0, $"use32\n");

                fixed (byte* pBytes = stackalloc byte[FASM_MEMORY_SIZE])
                {
                    if (FasmAssemble(Fasm.ToString(), pBytes, FASM_MEMORY_SIZE, FASM_PASSES, IntPtr.Zero) == 0)
                    {
                        FasmStateOk state = *(FasmStateOk*)pBytes;

                        if (patchMemProtection)
                        {
                            if (MemoryProtect(address, state.OutputLength, MemoryProtectionFlags.ExecuteReadWrite, out MemoryProtectionFlags oldMemoryProtection))
                            {
                                bool status = !NtWriteVirtualMemory(ProcessHandle, address, (void*)state.OutputData, (int)state.OutputLength, out _);
                                MemoryProtect(address, state.OutputLength, oldMemoryProtection, out _);

                                Fasm.Clear();
                                return status;
                            }
                        }
                        else
                        {
                            Fasm.Clear();
                            return !NtWriteVirtualMemory(ProcessHandle, address, (void*)state.OutputData, (int)state.OutputLength, out _);
                        }
                    }

                    // use this to read the error
                    FasmStateError stateError = *(FasmStateError*)pBytes;

                    Fasm.Clear();
                    return false;
                }
            }
        }

        /// <summary>
        /// Frees all allocated memory in the process.
        /// </summary>
        public void FreeAllMemory()
        {
            List<IntPtr> memAllocs = MemoryAllocations.Keys.ToList();

            for (int i = 0; i < memAllocs.Count; ++i)
            {
                FreeMemory(memAllocs[i]);
            }

            MemoryAllocations.Clear();
        }

        /// <summary>
        /// Free a memory allocation.
        /// </summary>
        /// <param name="address">Address of the allocation</param>
        /// <returns>True if freeing was successful, false if not</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool FreeMemory(IntPtr address)
        {
            lock (allocLock)
            {
                if (MemoryAllocations.ContainsKey(address)
                    && VirtualFreeEx(ProcessHandle, address, 0, AllocationTypes.Release))
                {
                    MemoryAllocations.Remove(address);
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Initializes XMemory. Recommended to use StartProcessNoActivate(), it will provide all variables needed.
        /// </summary>
        /// <param name="process">NET process class</param>
        /// <param name="processHandle">Native process handle</param>
        /// <param name="mainThreadHandle">Native thread handle</param>
        /// <returns>True if everything was set up correctly, false if not</returns>
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

            return true;
        }

        /// <summary>
        /// Change the memory protection of an area.
        /// </summary>
        /// <param name="address">Address to change</param>
        /// <param name="size">Size of the area</param>
        /// <param name="memoryProtection">New Protection</param>
        /// <param name="oldMemoryProtection">Old protection</param>
        /// <returns>True if it was successful, false if not</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MemoryProtect(IntPtr address, uint size, MemoryProtectionFlags memoryProtection, out MemoryProtectionFlags oldMemoryProtection)
        {
            return VirtualProtectEx(ProcessHandle, address, size, memoryProtection, out oldMemoryProtection);
        }

        /// <summary>
        /// Change and area of memory, by unprotecting it temporarily.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="address">Address to apply the patch</param>
        /// <param name="data">Data of the patch</param>
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

        /// <summary>
        /// Read an unmanaged type from the processes memory.
        /// </summary>
        /// <typeparam name="T">Type to read, can be any unmanaged type</typeparam>
        /// <param name="address">Address to read from</param>
        /// <param name="value">Value</param>
        /// <returns>True if reading was successful, false if not</returns>
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

        /// <summary>
        /// Read bytes from the processes memory.
        /// </summary>
        /// <param name="address">Address to read from</param>
        /// <param name="size">Size of the byte array to read</param>
        /// <param name="bytes">Bytes</param>
        /// <returns>True if reading was successful, false if not</returns>
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

        /// <summary>
        /// Read a string from the processes memory.
        /// </summary>
        /// <param name="address">Address to read from</param>
        /// <param name="encoding">Encoding to use</param>
        /// <param name="value">String</param>
        /// <param name="lenght">May lenght of the string</param>
        /// <returns>True if reading was successful, false if not</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReadString(IntPtr address, Encoding encoding, out string value, int lenght = 128)
        {
            fixed (byte* pBuffer = stackalloc byte[lenght])
            {
                if (RpmGateWay(address, pBuffer, lenght))
                {
                    List<byte> strBuffer = new(lenght);

                    for (int i = 0; i < lenght; ++i)
                    {
                        if (pBuffer[i] == 0) { break; }

                        strBuffer.Add(pBuffer[i]);
                    }

                    value = encoding.GetString(strBuffer.ToArray());
                    return true;
                }
            }

            value = string.Empty;
            return false;
        }

        /// <summary>
        /// Modifies the position of our parent window. See SetupAutoPosition() for more information.
        /// </summary>
        /// <param name="offsetX">Offset of the parent window</param>
        /// <param name="offsetY">Offset of the parent window</param>
        /// <param name="width">Width of the window</param>
        /// <param name="height">Height of the window</param>
        public void ResizeParentWindow(int offsetX, int offsetY, int width, int height)
        {
            SetWindowPos(Process.MainWindowHandle, IntPtr.Zero, offsetX, offsetY, width, height, SWP_NOZORDER | SWP_NOACTIVATE);
        }

        /// <summary>
        /// Resumes the main thread of the process.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ResumeMainThread()
        {
            NtResumeThread(MainThreadHandle, out _);
        }

        /// <summary>
        /// Makes the processes main window a parent of the supplied main window handle.
        /// </summary>
        /// <param name="mainWindowHandle">Master window</param>
        /// <param name="offsetX">Offset of the parent window</param>
        /// <param name="offsetY">Offset of the parent window</param>
        /// <param name="width">Width of the window</param>
        /// <param name="height">Height of the window</param>
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

        /// <summary>
        /// Suspend the main thread of the process.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SuspendMainThread()
        {
            NtSuspendThread(MainThreadHandle, out _);
        }

        /// <summary>
        /// Write an unmanaged value to the processes memory.
        /// </summary>
        /// <typeparam name="T">Type to write, can be any unmanaged type.</typeparam>
        /// <param name="address">Address to write to</param>
        /// <param name="value">Value to write</param>
        /// <returns>True if successful, false if not</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Write<T>(IntPtr address, T value) where T : unmanaged
        {
            return WpmGateWay(address, &value, sizeof(T));
        }

        /// <summary>
        /// Write bytes to the processes memory.
        /// </summary>
        /// <param name="address">Address to write to</param>
        /// <param name="bytes">Bytes to write</param>
        /// <returns>True if successful, false if not</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool WriteBytes(IntPtr address, byte[] bytes)
        {
            fixed (byte* pBytes = bytes)
            {
                return WpmGateWay(address, pBytes, bytes.Length);
            }
        }

        /// <summary>
        /// Set a memory region to 0.
        /// </summary>
        /// <param name="address">Address of the memory</param>
        /// <param name="size">Size of the area</param>
        /// <returns>True if successful, false if not</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ZeroMemory(IntPtr address, int size)
        {
            return WriteBytes(address, new byte[size]);
        }

        [DllImport("FASM.dll", EntryPoint = "fasm_Assemble", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        private static extern int FasmAssemble(string szSource, byte* lpMemory, int nSize, int nPassesLimit, IntPtr hDisplayPipe);

        [DllImport("FASM.dll", EntryPoint = "fasm_GetVersion", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        private static extern int FasmGetVersion();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool RpmGateWay(IntPtr baseAddress, void* buffer, int size)
        {
            ++rpmCalls;
            return !NtReadVirtualMemory(ProcessHandle, baseAddress, buffer, size, out _);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool WpmGateWay(IntPtr baseAddress, void* buffer, int size)
        {
            ++wpmCalls;
            return !NtWriteVirtualMemory(ProcessHandle, baseAddress, buffer, size, out _);
        }
    }
}