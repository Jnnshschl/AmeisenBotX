using AmeisenBotX.Common.Math;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow335a.Objects.Descriptors;
using System;

namespace AmeisenBotX.Wow335a.Objects
{
    [Serializable]
    public class WowDynobject335a : WowObject335a, IWowDynobject
    {
        public ulong Caster { get; set; }

        public float Radius { get; set; }

        public int SpellId { get; set; }

        public override string ToString()
        {
            return $"DynamicObject: [{Guid}] SpellId: {SpellId} Caster: {Caster} Radius: {Radius}";
        }

        public override void Update()
        {
            base.Update();

            if (Memory.Read(DescriptorAddress + WowObjectDescriptor335a.EndOffset, out WowDynobjectDescriptor335a objPtr)
                && Memory.Read(IntPtr.Add(BaseAddress, (int)Memory.Offsets.WowDynobjectPosition), out Vector3 position))
            {
                Caster = objPtr.Caster;
                Radius = objPtr.Radius;
                SpellId = objPtr.SpellId;
                Position = position;
            }
        }
    }
}