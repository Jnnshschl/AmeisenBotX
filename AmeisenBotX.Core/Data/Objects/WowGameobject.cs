using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.Raw;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System;
using System.Collections.Specialized;
using System.Globalization;

namespace AmeisenBotX.Core.Data.Objects
{
    public class WowGameobject : WowObject
    {
        public WowGameobject(IntPtr baseAddress, WowObjectType type, IntPtr descriptorAddress) : base(baseAddress, type, descriptorAddress)
        {
        }

        public byte Bytes0 { get; set; }

        public ulong CreatedBy { get; set; }

        public int DisplayId { get; set; }

        public int Faction { get; set; }

        public BitVector32 Flags { get; set; }

        public WowGameobjectType GameobjectType { get; set; }

        public int Level { get; set; }

        public override string ToString()
        {
            return $"GameObject: [{EntryId}] ({(Enum.IsDefined(typeof(WowGameobjectDisplayId), DisplayId) ? ((WowGameobjectDisplayId)DisplayId).ToString() : DisplayId.ToString(CultureInfo.InvariantCulture))}:{DisplayId})";
        }

        public override void Update(WowInterface wowInterface)
        {
            base.Update(wowInterface);

            if (wowInterface.XMemory.Read(DescriptorAddress + RawWowObject.EndOffset, out RawWowGameobject objPtr)
                && wowInterface.XMemory.Read(IntPtr.Add(BaseAddress, (int)wowInterface.OffsetList.WowGameobjectPosition), out Vector3 position))
            {
                GameobjectType = (WowGameobjectType)objPtr.GameobjectBytes1;
                CreatedBy = objPtr.CreatedBy;
                Bytes0 = objPtr.GameobjectBytes0;
                DisplayId = objPtr.DisplayId;
                Faction = objPtr.Faction;
                Flags = new(objPtr.Flags);
                Level = objPtr.Level;
                Position = position;
            }
        }
    }
}