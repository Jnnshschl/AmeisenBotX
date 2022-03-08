using AmeisenBotX.Common.Math;
using AmeisenBotX.Memory;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Offsets;
using AmeisenBotX.Wow548.Objects.Descriptors;

namespace AmeisenBotX.Wow548.Objects
{
    [Serializable]
    public class WowDynobject548 : WowObject548, IWowDynobject
    {
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

            if (memoryApi.Read(DescriptorAddress + WowObjectDescriptor548.EndOffset, out WowDynobjectDescriptor548 objPtr)
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