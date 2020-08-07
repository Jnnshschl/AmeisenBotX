using AmeisenBotX.Core.Data.Objects.WowObject.Structs;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System;

namespace AmeisenBotX.Core.Data.Objects.WowObject
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

        public unsafe override void Update()
        {
            base.Update();

            fixed (RawWowDynobject* objPtr = stackalloc RawWowDynobject[1])
            {
                if (WowInterface.I.XMemory.ReadStruct(DescriptorAddress + RawWowObject.EndOffset, objPtr)
                    && WowInterface.I.XMemory.ReadStruct(IntPtr.Add(BaseAddress, (int)WowInterface.I.OffsetList.WowDynobjectPosition), out Vector3 position))
                {
                    Caster = objPtr[0].Caster;
                    Radius = objPtr[0].Radius;
                    SpellId = objPtr[0].SpellId;
                    Position = position;
                }
            }
        }
    }
}