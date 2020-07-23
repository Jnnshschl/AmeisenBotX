using System;
using System.Runtime.InteropServices;

namespace AmeisenBotX.Core.Data.Objects.WowObject.Structs
{
    [Serializable]
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

        public const int EndOffset = 48;
    }
}