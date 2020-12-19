using AmeisenBotX.Core.Data.Objects.WowObjects.Structs;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System;

namespace AmeisenBotX.Core.Data.Objects.WowObjects
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

        public override unsafe void Update()
        {
            base.Update();

            if (WowInterface.I.XMemory.ReadStruct(DescriptorAddress + RawWowObject.EndOffset, out RawWowDynobject objPtr)
                && WowInterface.I.XMemory.ReadStruct(IntPtr.Add(BaseAddress, (int)WowInterface.I.OffsetList.WowDynobjectPosition), out Vector3 position))
            {
                Caster = objPtr.Caster;
                Radius = objPtr.Radius;
                SpellId = objPtr.SpellId;
                Position = position;
            }
        }
    }
}