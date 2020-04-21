using AmeisenBotX.Memory.Win32;
using Fasm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using static AmeisenBotX.Memory.Win32.Win32Imports;

namespace AmeisenBotX.Memory
{
    public class XMemory
    {
        public XMemory()
        {
            SizeCache = new Dictionary<Type, int>();
        }

        ~XMemory()
        {
            CloseHandle(MainThreadHandle);
        }

        public ManagedFasm Fasm { get; private set; }

        public IntPtr MainThreadHandle { get; private set; }

        public Process Process { get; private set; }

        public IntPtr ProcessHandle { get; private set; }

        private Dictionary<Type, int> SizeCache { get; }

        public static Rect GetWindowPosition(IntPtr windowHandle)
        {
            Rect rect = new Rect();
            GetWindowRect(windowHandle, ref rect);
            return rect;
        }

        public static void SetWindowPosition(IntPtr windowHandle, Rect rect, bool resizeWindow = true)
        {
            WindowFlags flags = resizeWindow ? (WindowFlags.AsyncWindowPos | WindowFlags.NoZOrder) : (WindowFlags.AsyncWindowPos | WindowFlags.NoZOrder | WindowFlags.NoSize);

            if (rect.Left > 0 && rect.Right > 0 && rect.Top > 0 && rect.Bottom > 0)
            {
                SetWindowPos(windowHandle, 0, rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top, (int)flags);
            }
        }

        public bool AllocateMemory(uint size, out IntPtr address)
        {
            try
            {
                address = VirtualAllocEx(ProcessHandle, IntPtr.Zero, size, AllocationType.Commit, MemoryProtection.ExecuteReadWrite);
            }
            catch
            {
                address = IntPtr.Zero;
            }

            return address != IntPtr.Zero;
        }

        public void Attach(Process wowProcess)
        {
            Process = wowProcess;
            ProcessHandle = OpenProcess(ProcessAccessFlags.All, false, wowProcess.Id);
            Fasm = new ManagedFasm(ProcessHandle);
        }

        public bool FreeMemory(IntPtr address)
        {
            try
            {
                return VirtualFreeEx(ProcessHandle, address, 0, AllocationType.Release);
            }
            catch
            {
                return false;
            }
        }

        public ProcessThread GetMainThread()
        {
            if (Process.MainWindowHandle == null)
            {
                return null;
            }

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

        public unsafe bool Read<T>(IntPtr address, out T value) where T : unmanaged
        {
            int size = sizeof(T);
            byte[] readBuffer = new byte[size];

            try
            {
                if (ReadProcessMemory(ProcessHandle, address, readBuffer, size, out _))
                {
                    fixed (byte* ptr = readBuffer)
                    {
                        value = *(T*)ptr;
                        return true;
                    }
                }
            }
            catch
            {
                // ignored lel
            }

            value = default;
            return false;
        }

        public unsafe bool ReadByte(IntPtr address, out byte buffer)
        {
            byte[] readBuffer = new byte[1];

            if (ReadProcessMemory(ProcessHandle, address, readBuffer, 1, out _))
            {
                fixed (byte* ptr = readBuffer)
                {
                    buffer = *ptr;
                    return true;
                }
            }

            buffer = 0x0;
            return false;
        }

        public bool ReadBytes(IntPtr address, int size, out byte[] bytes)
        {
            byte[] readBuffer = new byte[size];

            if (ReadProcessMemory(ProcessHandle, address, readBuffer, size, out _))
            {
                bytes = readBuffer;
                return true;
            }

            bytes = new byte[size];
            return false;
        }

        public unsafe bool ReadInt(IntPtr address, out int value)
        {
            byte[] readBuffer = new byte[4];

            try
            {
                if (ReadProcessMemory(ProcessHandle, address, readBuffer, 4, out _))
                {
                    fixed (byte* ptr = readBuffer)
                    {
                        value = *(int*)ptr;
                        return true;
                    }
                }
            }
            catch
            {
                // ignored lel
            }

            value = 0;
            return false;
        }

        public bool ReadString(IntPtr address, Encoding encoding, out string value, int lenght = 128)
        {
            byte[] readBuffer = new byte[lenght];

            try
            {
                if (ReadProcessMemory(ProcessHandle, address, readBuffer, lenght, out _))
                {
                    List<byte> stringBytes = new List<byte>();

                    foreach (byte b in readBuffer)
                    {
                        if (b == 0b0)
                        {
                            break;
                        }

                        stringBytes.Add(b);
                    }

                    value = encoding.GetString(stringBytes.ToArray()).Trim();
                    return true;
                }
            }
            catch
            {
                // ignored lel
            }

            value = string.Empty;
            return false;
        }

        public bool ReadStruct<T>(IntPtr address, out T value)
        {
            int size = SizeOf(typeof(T));
            IntPtr readBuffer = Marshal.AllocHGlobal(size);

            try
            {
                if (ReadProcessMemory(ProcessHandle, address, readBuffer, size, out _))
                {
                    value = (T)Marshal.PtrToStructure(readBuffer, typeof(T));
                    return true;
                }
            }
            finally
            {
                Marshal.FreeHGlobal(readBuffer);
            }

            value = default;
            return false;
        }

        public void ResumeMainThread()
        {
            if (OpenMainThread())
            {
                ResumeThread(MainThreadHandle);
            }
        }

        public bool SetMemory(IntPtr address, int size, int value)
        {
            try
            {
                address = MemSet(address, value, size);
                return true;
            }
            catch { }

            return false;
        }

        public void SuspendMainThread()
        {
            if (OpenMainThread())
            {
                SuspendThread(MainThreadHandle);
            }
        }

        public bool Write<T>(IntPtr address, T value, int size = 0)
        {
            if (size == 0)
            {
                size = SizeOf(typeof(T));
            }

            IntPtr writeBuffer = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(value, writeBuffer, false);

            bool result = WriteProcessMemory(ProcessHandle, address, writeBuffer, size, out _);

            Marshal.DestroyStructure(writeBuffer, typeof(T));
            Marshal.FreeHGlobal(writeBuffer);

            return result;
        }

        public bool WriteBytes(IntPtr address, byte[] bytes)
            => WriteProcessMemory(ProcessHandle, address, bytes, bytes.Length, out _);

        public bool ZeroMemory(IntPtr address, int size)
                                    => WriteBytes(address, new byte[size]);

        private bool OpenMainThread()
        {
            if (MainThreadHandle == IntPtr.Zero)
            {
                ProcessThread mainThread = GetMainThread();
                MainThreadHandle = OpenThread(ThreadAccess.SuspendResume, false, (uint)mainThread.Id);
                return MainThreadHandle != null && MainThreadHandle != IntPtr.Zero;
            }

            return true;
        }

        private int SizeOf(Type type)
        {
            if (!SizeCache.ContainsKey(type))
            {
                DynamicMethod dm = new DynamicMethod("SizeOf", typeof(int), new Type[] { });
                ILGenerator il = dm.GetILGenerator();

                il.Emit(OpCodes.Sizeof, type);
                il.Emit(OpCodes.Ret);

                int size = (int)dm.Invoke(null, null);
                SizeCache.Add(type, size);
            }

            return SizeCache[type];
        }
    }
}