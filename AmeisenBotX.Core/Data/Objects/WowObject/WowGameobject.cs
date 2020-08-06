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

        public byte Bytes0 { get; set; }

        public int DisplayId { get; set; }

        public int Faction { get; set; }

        public BitVector32 Flags { get; set; }

        public WowGameobjectType GameobjectType { get; set; }

        public int Level { get; set; }

        public override string ToString()
        {
            return $"GameObject: [{EntryId}] ({(Enum.IsDefined(typeof(GameobjectDisplayId), DisplayId) ? ((GameobjectDisplayId)DisplayId).ToString() : DisplayId.ToString())}:{DisplayId})";
        }

        public WowGameobject UpdateRawWowGameobject()
        {
            UpdateRawWowObject();

            unsafe
            {
                fixed (RawWowGameobject* objPtr = stackalloc RawWowGameobject[1])
                {
                    if (WowInterface.I.XMemory.ReadStruct(DescriptorAddress + RawWowObject.EndOffset, objPtr))
                    {
                        GameobjectType = (WowGameobjectType)objPtr[0].GameobjectBytes1;
                        Bytes0 = objPtr[0].GameobjectBytes0;
                        DisplayId = objPtr[0].DisplayId;
                        Faction = objPtr[0].Faction;
                        Flags = new BitVector32(objPtr[0].Flags);
                        Level = objPtr[0].Level;
                    }
                }
            }

            return this;
        }
    }
}