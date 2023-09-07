using System;

namespace AmeisenBotX.Memory.Win32
{

    [Flags]
    public enum WindowFlag
    {
        NoSize = 0x1,
        NoMove = 0x2,
        NoZOrder = 0x4,
        NoRedraw = 0x8,
        NoActivate = 0x10,
        DrawFrame = 0x20,
        ShowWindow = 0x40,
        HideWindow = 0x80,
        NoCopyBits = 0x100,
        NoOwnerZOrder = 0x200,
        NoSendChanging = 0x400,
        Defererase = 0x2000,
        AsyncWindowPos = 0x4000
    }

}