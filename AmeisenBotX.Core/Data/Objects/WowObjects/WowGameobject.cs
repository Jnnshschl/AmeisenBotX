using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObjects.Structs;
using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Numerics;

namespace AmeisenBotX.Core.Data.Objects.WowObjects
{
    public class WowGameobject : WowObject
    {
        public WowGameobject(IntPtr baseAddress, WowObjectType type, IntPtr descriptorAddress) : base(baseAddress, type, descriptorAddress)
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
            return $"GameObject: [{EntryId}] ({(Enum.IsDefined(typeof(GameobjectDisplayId), DisplayId) ? ((GameobjectDisplayId)DisplayId).ToString() : DisplayId.ToString(CultureInfo.InvariantCulture))}:{DisplayId})";
        }

        public override unsafe void Update()
        {
            base.Update();

            if (WowInterface.I.XMemory.ReadStruct(DescriptorAddress + RawWowObject.EndOffset, out RawWowGameobject objPtr)
                && WowInterface.I.XMemory.ReadStruct(IntPtr.Add(BaseAddress, (int)WowInterface.I.OffsetList.WowGameobjectPosition), out Vector3 position))
            {
                GameobjectType = (WowGameobjectType)objPtr.GameobjectBytes1;
                Bytes0 = objPtr.GameobjectBytes0;
                DisplayId = objPtr.DisplayId;
                Faction = objPtr.Faction;
                Flags = new BitVector32(objPtr.Flags);
                Level = objPtr.Level;
                Position = position;
            }
        }
    }
}