using AmeisenBotX.Common.Math;
using AmeisenBotX.Memory;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Wow.Offsets;
using AmeisenBotX.Wow335a.Objects.Descriptors;
using System;
using System.Collections.Specialized;
using System.Globalization;

namespace AmeisenBotX.Wow335a.Objects
{
    [Serializable]
    public class WowGameobject335a : WowObject335a, IWowGameobject
    {
        public WowGameobject335a(IntPtr baseAddress, IntPtr descriptorAddress) : base(baseAddress, descriptorAddress)
        {
            Type = WowObjectType.GameObject;
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

        public override void Update(IMemoryApi memoryApi, IOffsetList offsetList)
        {
            base.Update(memoryApi, offsetList);

            if (memoryApi.Read(DescriptorAddress + WowObjectDescriptor.EndOffset, out WowGameobjectDescriptor objPtr)
                && memoryApi.Read(IntPtr.Add(BaseAddress, (int)offsetList.WowGameobjectPosition), out Vector3 position))
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