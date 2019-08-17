﻿using AmeisenBotX.Memory.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static AmeisenBotX.Memory.Win32.Win32Imports;

namespace AmeisenBotX.Memory
{
    public class XMemory
    {
        public Process Process { get; private set; }
        public IntPtr ProcessHandle { get; private set; }

        private Dictionary<Type, int> SizeCache { get; }

        public XMemory()
        {
            SizeCache = new Dictionary<Type, int>();
        }

        public void Attach(Process wowProcess)
        {
            Process = wowProcess;
            ProcessHandle = OpenProcess(ProcessAccessFlags.All, false, wowProcess.Id);
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
            catch { }

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
                            break;

                        stringBytes.Add(b);
                    }

                    value = encoding.GetString(stringBytes.ToArray()).Trim();
                    return true;
                }
            }
            catch { }

            value = "";
            return false;
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
            catch { }

            value = default;
            return false;
        }

        public bool ReadStruct<T>(IntPtr address, out T value)
        {
            int size = Marshal.SizeOf(typeof(T));
            IntPtr readBuffer = Marshal.AllocHGlobal(size);

            try
            {
                if (ReadProcessMemory(ProcessHandle, address, readBuffer, size, out _))
                {
                    value = (T)Marshal.PtrToStructure(readBuffer, typeof(T));
                    return true;
                }
            }
            catch { }
            finally
            {
                Marshal.FreeHGlobal(readBuffer);
            }

            value = default;
            return false;
        }

        public bool Read(object staticClass)
        {
            throw new NotImplementedException();
        }
    }
}