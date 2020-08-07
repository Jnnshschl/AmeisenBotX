using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject.Structs;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System;
using System.Collections.Specialized;

namespace AmeisenBotX.Core.Data.Objects.WowObject
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
            return $"GameObject: [{EntryId}] ({(Enum.IsDefined(typeof(GameobjectDisplayId), DisplayId) ? ((GameobjectDisplayId)DisplayId).ToString() : DisplayId.ToString())}:{DisplayId})";
        }

        public unsafe override void Update()
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