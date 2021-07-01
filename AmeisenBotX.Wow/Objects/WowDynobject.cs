using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Offsets;
using AmeisenBotX.Memory;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System;

namespace AmeisenBotX.Core.Data.Objects
{
    [Serializable]
    public class WowDynobject : WowObject
    {
        public WowDynobject(IntPtr baseAddress, WowObjectType type, IntPtr descriptorAddress) : base(baseAddress, type, descriptorAddress)
        {
        }

        public ulong Caster { get; set; }

        public float Radius { get; set; }

        public int SpellId { get; set; }

        public override string ToString()
        {
            return $"DynamicObject: [{Guid}] SpellId: {SpellId} Caster: {Caster} Radius: {Radius}";
        }

        public override void Update(IMemoryApi memoryApi, IOffsetList offsetList)
        {
            base.Update(memoryApi, offsetList);

            if (memoryApi.Read(DescriptorAddress + RawWowObject.EndOffset, out RawWowDynobject objPtr)
                && memoryApi.Read(IntPtr.Add(BaseAddress, (int)offsetList.WowDynobjectPosition), out Vector3 position))
            {
                Caster = objPtr.Caster;
                Radius = objPtr.Radius;
                SpellId = objPtr.SpellId;
                Position = position;
            }
        }
    }
}