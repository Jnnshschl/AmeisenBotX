using System;

namespace AmeisenBotX.Memory.Win32
{
    [Flags]
    public enum ThreadAccessFlag
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

}