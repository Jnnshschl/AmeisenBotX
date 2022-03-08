using AmeisenBotX.Wow.Objects.Enums;

namespace AmeisenBotX.Wow.Objects
{
    public interface IWowContainer : IWowObject
    {
        int SlotCount { get; }

        public new WowObjectType Type => WowObjectType.Container;
    }
}