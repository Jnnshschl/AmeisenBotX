using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow335a.Objects.Descriptors;
using System;

namespace AmeisenBotX.Wow335a.Objects
{
    [Serializable]
    public class WowContainer335a : WowObject335a, IWowContainer
    {
        public int SlotCount => RawWowContainer.SlotCount;

        protected WowContainerDescriptor335a RawWowContainer { get; private set; }

        public override string ToString()
        {
            return $"Container: [{Guid}] SlotCount: {SlotCount}";
        }

        public override void Update()
        {
            base.Update();

            if (Memory.Read(DescriptorAddress + WowObjectDescriptor335a.EndOffset, out WowContainerDescriptor335a obj))
            {
                RawWowContainer = obj;
            }
        }
    }
}