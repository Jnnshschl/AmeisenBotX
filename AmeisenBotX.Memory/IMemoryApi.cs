using AmeisenBotX.Memory.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using static AmeisenBotX.Memory.Win32.Win32Imports;

namespace AmeisenBotX.Memory
{
    public interface IMemoryApi : IDisposable
    {
        /// <summary>
        /// Native handle for the main thread.
        /// </summary>
        public IntPtr MainThreadHandle { get; }

        /// <summary>
        /// All memory allocations.
        /// </summary>
        public Dictionary<IntPtr, uint> MemoryAllocations { get; }

        /// <summary>
        /// NET process class.
        /// </summary>
        public Process Process { get; }

        /// <summary>
        /// Native process handle.
        /// </summary>
        public IntPtr ProcessHandle { get; }

        /// <summary>
        /// Get the amount of ReadProcessMemory calls since last time.
        /// </summary>
        public ulong RpmCallCount { get; }

        /// <summary>
        /// Get the amount of WriteProcessMemory calls since last time.
        /// </summary>
        public ulong WpmCallCount { get; }

        /// <summary>
        /// Allocate memory in the process.
        /// </summary>
        /// <param name="size">Allocation size in bytes</param>
        /// <param name="address">Address of the allocation</param>
        /// <returns>True if allocation was successful, false if not</returns>
        public bool AllocateMemory(uint size, out IntPtr address);

        /// <summary>
        /// Disposes XMemory and frees all memory allocated by it.
        /// </summary>
        public new void Dispose();

        /// <summary>
        /// Brings a window to the users focus.
        /// </summary>
        /// <param name="windowHandle">Window Handle</param>
        /// <param name="rect">Position of the window</param>
        /// <param name="resizeWindow">True when you want to resize the window, false if not</param>
        public void FocusWindow(IntPtr windowHandle, Rect rect, bool resizeWindow = true);

        /// <summary>
        /// Frees all allocated memory in the process.
        /// </summary>
        public void FreeAllMemory();

        /// <summary>
        /// Free a memory allocation.
        /// </summary>
        /// <param name="address">Address of the allocation</param>
        /// <returns>True if freeing was successful, false if not</returns>
        public bool FreeMemory(IntPtr address);

        /// <summary>
        /// Returns the client window rect of the given window.
        /// </summary>
        /// <param name="windowHandle">Window handle</param>
        /// <returns>The size of the client area of the window.</returns>
        public Rect GetClientSize();

        /// <summary>
        /// Get the current focused window handle.
        /// </summary>
        /// <returns>Window handle</returns>
        public IntPtr GetForegroundWindow();

        /// <summary>
        /// Returns the position of the supplied window.
        /// </summary>
        /// <param name="windowHandle">Window handle</param>
        /// <returns>Window position</returns>
        public Rect GetWindowPosition();

        /// <summary>
        /// Initializes XMemory. Recommended to use StartProcessNoActivate(), it will provide all variables needed.
        /// </summary>
        /// <param name="process">NET process class</param>
        /// <param name="processHandle">Native process handle</param>
        /// <param name="mainThreadHandle">Native thread handle</param>
        /// <returns>True if everything was set up correctly, false if not</returns>
        public bool Init(Process process, IntPtr processHandle, IntPtr mainThreadHandle);

        /// <summary>
        /// Injects the current Fasm buffer and clears it.
        /// </summary>
        /// <param name="asm">Assembly to inject</param>
        /// <param name="address">Address where the fasm will be injected</param>
        /// <param name="patchMemProtection">Whether we need to patch memory protection or nor. Useful when memory is write protected.</param>
        /// <returns>True when the injection was successful</returns>
        public bool InjectAssembly(IEnumerable<string> asm, IntPtr address, bool patchMemProtection = false);

        /// <summary>
        /// Change and area of memory, by unprotecting it temporarily.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="address">Address to apply the patch</param>
        /// <param name="data">Data of the patch</param>
        void PatchMemory<T>(IntPtr address, T data) where T : unmanaged;

        /// <summary>
        /// Change the memory protection of an area.
        /// </summary>
        /// <param name="address">Address to change</param>
        /// <param name="size">Size of the area</param>
        /// <param name="memoryProtection">New Protection</param>
        /// <param name="oldMemoryProtection">Old protection</param>
        /// <returns>True if it was successful, false if not</returns>
        public bool ProtectMemory(IntPtr address, uint size, MemoryProtectionFlag memoryProtection, out MemoryProtectionFlag oldMemoryProtection);

        /// <summary>
        /// Read an unmanaged type from the processes memory.
        /// </summary>
        /// <typeparam name="T">Type to read, can be any unmanaged type</typeparam>
        /// <param name="address">Address to read from</param>
        /// <param name="value">Value</param>
        /// <returns>True if reading was successful, false if not</returns>
        public bool Read<T>(IntPtr address, out T value) where T : unmanaged;

        /// <summary>
        /// Read bytes from the processes memory.
        /// </summary>
        /// <param name="address">Address to read from</param>
        /// <param name="size">Size of the byte array to read</param>
        /// <param name="bytes">Bytes</param>
        /// <returns>True if reading was successful, false if not</returns>
        public bool ReadBytes(IntPtr address, int size, out byte[] bytes);

        /// <summary>
        /// Read a string from the processes memory.
        /// </summary>
        /// <param name="address">Address to read from</param>
        /// <param name="encoding">Encoding to use</param>
        /// <param name="value">String</param>
        /// <param name="bufferSize">Max bytes to read per cycle</param>
        /// <returns>True if reading was successful, false if not</returns>
        public bool ReadString(IntPtr address, Encoding encoding, out string value, int bufferSize = 512);

        /// <summary>
        /// Modifies the position of our parent window. See SetupAutoPosition() for more information.
        /// </summary>
        /// <param name="offsetX">Offset of the parent window</param>
        /// <param name="offsetY">Offset of the parent window</param>
        /// <param name="width">Width of the window</param>
        /// <param name="height">Height of the window</param>
        public void ResizeParentWindow(int offsetX, int offsetY, int width, int height);

        /// <summary>
        /// Resumes the main thread of the process.
        /// </summary>
        public void ResumeMainThread();

        /// <summary>
        /// Focus the specified window.
        /// </summary>
        /// <param name="windowHandle">Window handle</param>
        public void SetForegroundWindow(IntPtr windowHandle);

        /// <summary>
        /// Makes the processes main window a parent of the supplied main window handle.
        /// </summary>
        /// <param name="mainWindowHandle">Master window</param>
        /// <param name="offsetX">Offset of the parent window</param>
        /// <param name="offsetY">Offset of the parent window</param>
        /// <param name="width">Width of the window</param>
        /// <param name="height">Height of the window</param>
        public void SetupAutoPosition(IntPtr mainWindowHandle, int offsetX, int offsetY, int width, int height);

        /// <summary>
        /// Set the position of a window.
        /// </summary>
        /// <param name="windowHandle">Window handle</param>
        /// <param name="rect">Position</param>
        /// <param name="resizeWindow">Should we resize the window?</param>
        public void SetWindowPosition(IntPtr windowHandle, Rect rect, bool resizeWindow = true);

        /// <summary>
        /// Start a process but dont focus its window.
        /// </summary>
        /// <param name="processCmd">Command to run</param>
        /// <param name="processHandle">The native process handle of the started process</param>
        /// <param name="threadHandle">The native thread handle of the started process</param>
        /// <returns>Process object</returns>
        public Process StartProcessNoActivate(string processCmd, out IntPtr processHandle, out IntPtr threadHandle);

        /// <summary>
        /// Suspend the main thread of the process.
        /// </summary>
        void SuspendMainThread();

        /// <summary>
        /// Write an unmanaged value to the processes memory.
        /// </summary>
        /// <typeparam name="T">Type to write, can be any unmanaged type.</typeparam>
        /// <param name="address">Address to write to</param>
        /// <param name="value">Value to write</param>
        /// <returns>True if successful, false if not</returns>
        bool Write<T>(IntPtr address, T value) where T : unmanaged;

        /// <summary>
        /// Write bytes to the processes memory.
        /// </summary>
        /// <param name="address">Address to write to</param>
        /// <param name="bytes">Bytes to write</param>
        /// <returns>True if successful, false if not</returns>
        bool WriteBytes(IntPtr address, byte[] bytes);

        /// <summary>
        /// Set a memory region to 0.
        /// </summary>
        /// <param name="address">Address of the memory</param>
        /// <param name="size">Size of the area</param>
        /// <returns>True if successful, false if not</returns>
        bool ZeroMemory(IntPtr address, int size);
    }
}