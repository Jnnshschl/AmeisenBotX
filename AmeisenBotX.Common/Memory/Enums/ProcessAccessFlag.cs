using System;

namespace AmeisenBotX.Memory.Win32
{
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

}