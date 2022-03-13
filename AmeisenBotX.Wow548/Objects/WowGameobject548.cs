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
    public unsafe class WowGameobject548 : WowObject548, IWowGameobject
    {
        protected WowGameobjectDescriptor548? GameobjectDescriptor;

        public byte Bytes0 { get; set; }

        public ulong CreatedBy => GetGameobjectDescriptor().CreatedBy;

        public int DisplayId => GetGameobjectDescriptor().DisplayId;

        public int Faction => GetGameobjectDescriptor().FactionTemplate;

        public BitVector32 Flags => GetGameobjectDescriptor().Flags;

        public WowGameObjectType GameObjectType { get; set; }

        public int Level => GetGameobjectDescriptor().Level;

        public new Vector3 Position => Memory.Read(IntPtr.Add(BaseAddress, (int)Offsets.WowGameobjectPosition), out Vector3 position) ? position : Vector3.Zero;

        public override string ToString()
        {
            return $"GameObject: [{EntryId}] ({(Enum.IsDefined(typeof(WowGameObjectDisplayId), DisplayId) ? ((WowGameObjectDisplayId)DisplayId).ToString() : DisplayId.ToString(CultureInfo.InvariantCulture))}:{DisplayId})";
        }

        public override void Update(IMemoryApi memoryApi, IOffsetList offsetList)
        {
            base.Update(memoryApi, offsetList);

            // GameObjectType = (WowGameObjectType)objPtr.GameobjectBytes1; Bytes0 = objPtr.GameobjectBytes0;
        }

        protected WowGameobjectDescriptor548 GetGameobjectDescriptor() => GameobjectDescriptor ??= Memory.Read(DescriptorAddress + sizeof(WowObjectDescriptor548), out WowGameobjectDescriptor548 objPtr) ? objPtr : new();
    }
}