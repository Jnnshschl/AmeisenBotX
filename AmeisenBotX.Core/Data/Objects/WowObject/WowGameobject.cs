using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject.Structs;
using AmeisenBotX.Memory;
using System;
using System.Collections.Specialized;

namespace AmeisenBotX.Core.Data.Objects.WowObject
{
    public class WowGameobject : WowObject
    {
        public WowGameobject(IntPtr baseAddress, WowObjectType type) : base(baseAddress, type)
        {
        }

        public int DisplayId => RawWowGameobject.DisplayId;

        public BitVector32 DynamicFlags { get; set; }

        public int Faction => RawWowGameobject.Faction;

        public BitVector32 Flags => new BitVector32(RawWowGameobject.Flags);

        public WowGameobjectType GameobjectType => (WowGameobjectType)RawWowGameobject.GameobjectBytes1;

        public int Level => RawWowGameobject.Level;

        private RawWowGameobject RawWowGameobject { get; set; }

        public override string ToString()
        {
            if (Enum.IsDefined(typeof(GameobjectDisplayId), DisplayId))
            {
                return $"GameObject: [{EntryId}] ({((GameobjectDisplayId)DisplayId)}:{DisplayId})";
            }
            else
            {
                return $"GameObject: [{EntryId}] ({DisplayId})";
            }
        }

        public WowGameobject UpdateRawWowGameobject(XMemory xMemory)
        {
            UpdateRawWowObject(xMemory);

            if (xMemory.ReadStruct(DescriptorAddress + RawWowObject.EndOffset, out RawWowGameobject rawWowGameobject))
            {
                RawWowGameobject = rawWowGameobject;
            }

            return this;
        }
    }
}