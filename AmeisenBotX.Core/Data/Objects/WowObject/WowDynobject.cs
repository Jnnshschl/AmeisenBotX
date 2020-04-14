using AmeisenBotX.Core.Data.Objects.WowObject.Structs;
using AmeisenBotX.Memory;
using System;

namespace AmeisenBotX.Core.Data.Objects.WowObject
{
    public class WowDynobject : WowObject
    {
        public WowDynobject(IntPtr baseAddress, WowObjectType type) : base(baseAddress, type)
        {
        }

        public ulong Caster => RawWowDynobject.Caster;

        public float Radius => RawWowDynobject.Radius;

        public int SpellId => RawWowDynobject.SpellId;

        private RawWowDynobject RawWowDynobject { get; set; }

        public override string ToString()
            => $"DynamicObject: [{Guid}] SpellId: {SpellId} Caster: {Caster} Radius: {Radius}";

        public WowDynobject UpdateRawWowDynobject(XMemory xMemory)
        {
            UpdateRawWowObject(xMemory);

            if (xMemory.ReadStruct(DescriptorAddress + RawWowObject.EndOffset, out RawWowDynobject rawWowDynobject))
            {
                RawWowDynobject = rawWowDynobject;
            }

            return this;
        }
    }
}