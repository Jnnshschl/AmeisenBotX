using AmeisenBotX.Common.Math;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow548.Objects.Descriptors;

namespace AmeisenBotX.Wow548.Objects
{
    [Serializable]
    public unsafe class WowDynobject548 : WowObject548, IWowDynobject
    {
        protected WowDynamicobjectDescriptor548? DynamicobjectDescriptor;

        public ulong Caster => GetDynamicobjectDescriptor().Caster;

        public new Vector3 Position => Memory.Read(IntPtr.Add(BaseAddress, (int)Memory.Offsets.WowDynobjectPosition), out Vector3 position) ? position : Vector3.Zero;

        public float Radius => GetDynamicobjectDescriptor().Radius;

        public int SpellId => GetDynamicobjectDescriptor().SpellId;

        public override string ToString()
        {
            return $"DynamicObject: [{Guid}] SpellId: {SpellId} Caster: {Caster} Radius: {Radius}";
        }

        public override void Update()
        {
            base.Update();
        }

        protected WowDynamicobjectDescriptor548 GetDynamicobjectDescriptor()
        {
            return DynamicobjectDescriptor ??= Memory.Read(DescriptorAddress + sizeof(WowObjectDescriptor548), out WowDynamicobjectDescriptor548 objPtr) ? objPtr : new();
        }
    }
}