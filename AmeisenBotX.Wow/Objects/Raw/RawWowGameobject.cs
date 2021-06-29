﻿using System.Runtime.InteropServices;

namespace AmeisenBotX.Wow.Objects
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct RawWowGameobject
    {
        public ulong CreatedBy;
        public int DisplayId;
        public int Flags;
        public fixed float ParentRotations[4];
        public fixed short Dynamics[2];
        public int Faction;
        public int Level;
        public byte GameobjectBytes0;
        public byte GameobjectBytes1;
        public byte GameobjectBytes2;
        public byte GameobjectBytes3;

        public static readonly int EndOffset = 48;
    }
}