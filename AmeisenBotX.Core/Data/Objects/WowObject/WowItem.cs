using AmeisenBotX.Core.Data.Objects.WowObject.Structs;
using AmeisenBotX.Memory;

namespace AmeisenBotX.Core.Data.Objects.WowObject
{
    public class WowItem : WowObject
    {
        // W.I.P
        // This is going to reduce the ReadProcessMemory calls a lot
        // public WowItem(IntPtr baseAddress, WowObjectType type = WowObjectType.None) : base(baseAddress, type)
        // {
        //
        // }

        public override int BaseOffset => base.BaseOffset + RawWowObject.EndOffset;

        private RawWowItem RawWowItem { get; set; }

        public override WowObject Update(XMemory xMemory)
        {
            base.Update(xMemory);

            if (xMemory.ReadStruct(BaseAddress, out RawWowItem rawWowItem))
            {
                RawWowItem = rawWowItem;
            }

            return this;
        }
    }
}