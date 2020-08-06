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

        public byte Bytes0 => RawWowGameobject.GameobjectBytes0;

        public int DisplayId => RawWowGameobject.DisplayId;

        public BitVector32 DynamicFlags { get; set; }

        public int Faction => RawWowGameobject.Faction;

        public BitVector32 Flags => new BitVector32(RawWowGameobject.Flags);

        public WowGameobjectType GameobjectType => (WowGameobjectType)RawWowGameobject.GameobjectBytes1;

        public int Level => RawWowGameobject.Level;

        private RawWowGameobject RawWowGameobject { get; set; }

        public override string ToString()
        {
            return $"GameObject: [{EntryId}] ({(Enum.IsDefined(typeof(GameobjectDisplayId), DisplayId) ? ((GameobjectDisplayId)DisplayId).ToString() : DisplayId.ToString())}:{DisplayId})";
        }

        public WowGameobject UpdateRawWowGameobject()
        {
            UpdateRawWowObject();

            if (WowInterface.I.XMemory.ReadStruct(DescriptorAddress + RawWowObject.EndOffset, out RawWowGameobject rawWowGameobject))
            {
                RawWowGameobject = rawWowGameobject;
            }

            return this;
        }
    }
}