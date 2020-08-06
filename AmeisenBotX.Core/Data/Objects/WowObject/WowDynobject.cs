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

        private RawWowDynobject RawWowDynobject { get; set; }

        public override string ToString()
        {
            return $"DynamicObject: [{Guid}] SpellId: {SpellId} Caster: {Caster} Radius: {Radius}";
        }

        public WowDynobject UpdateRawWowDynobject()
        {
            UpdateRawWowObject();

            if (WowInterface.I.XMemory.ReadStruct(DescriptorAddress + RawWowObject.EndOffset, out RawWowDynobject rawWowDynobject))
            {
                Caster = rawWowDynobject.Caster;
                Radius = rawWowDynobject.Radius;
                SpellId = rawWowDynobject.SpellId;
            }

            return this;
        }
    }
}