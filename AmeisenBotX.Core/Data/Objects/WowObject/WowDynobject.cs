using AmeisenBotX.Core.Data.Objects.WowObject.Structs;
using AmeisenBotX.Memory;
using System;

namespace AmeisenBotX.Core.Data.Objects.WowObject
{
    [Serializable]
    public class WowDynobject : WowObject
    {
        public WowDynobject(IntPtr baseAddress, WowObjectType type) : base(baseAddress, type)
        {
        }

        public ulong Caster { get; set; }

        public float Radius { get; set; }

        public int SpellId { get; set; }

        public override string ToString()
        {
            return $"DynamicObject: [{Guid}] SpellId: {SpellId} Caster: {Caster} Radius: {Radius}";
        }

        public WowDynobject UpdateRawWowDynobject()
        {
            UpdateRawWowObject();

            unsafe
            {
                fixed (RawWowDynobject* objPtr = stackalloc RawWowDynobject[1])
                {
                    if (WowInterface.I.XMemory.ReadStruct(DescriptorAddress + RawWowObject.EndOffset, objPtr))
                    {
                        Caster = objPtr[0].Caster;
                        Radius = objPtr[0].Radius;
                        SpellId = objPtr[0].SpellId;
                    }
                }
            }

            return this;
        }
    }
}