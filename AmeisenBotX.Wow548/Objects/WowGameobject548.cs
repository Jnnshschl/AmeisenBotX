using AmeisenBotX.Common.Math;
using AmeisenBotX.Memory;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Wow.Offsets;
using AmeisenBotX.Wow548.Objects.Descriptors;
using System.Collections.Specialized;
using System.Globalization;

namespace AmeisenBotX.Wow548.Objects
{
    [Serializable]
    public class WowGameobject548 : WowObject548, IWowGameobject
    {
        public WowGameobject548(IntPtr baseAddress, IntPtr descriptorAddress) : base(baseAddress, descriptorAddress)
        {
            Type = WowObjectType.GameObject;
        }

        public byte Bytes0 { get; set; }

        public ulong CreatedBy { get; set; }

        public int DisplayId { get; set; }

        public int Faction { get; set; }

        public BitVector32 Flags { get; set; }

        public WowGameObjectType GameObjectType { get; set; }

        public int Level { get; set; }

        public override string ToString()
        {
            return $"GameObject: [{EntryId}] ({(Enum.IsDefined(typeof(WowGameObjectDisplayId), DisplayId) ? ((WowGameObjectDisplayId)DisplayId).ToString() : DisplayId.ToString(CultureInfo.InvariantCulture))}:{DisplayId})";
        }

        public override void Update(IMemoryApi memoryApi, IOffsetList offsetList)
        {
            base.Update(memoryApi, offsetList);

            if (memoryApi.Read(DescriptorAddress + WowObjectDescriptor548.EndOffset, out WowGameobjectDescriptor548 objPtr)
                && memoryApi.Read(IntPtr.Add(BaseAddress, (int)offsetList.WowGameobjectPosition), out Vector3 position))
            {
                // GameObjectType = (WowGameObjectType)objPtr.GameobjectBytes1;
                CreatedBy = objPtr.CreatedBy;
                // Bytes0 = objPtr.GameobjectBytes0;
                DisplayId = objPtr.DisplayId;
                Faction = objPtr.Faction;
                Flags = new(objPtr.Flags);
                Level = objPtr.Level;
                Position = position;
            }
        }
    }
}