using AmeisenBotX.Wow.Objects.Enums;

namespace AmeisenBotX.Wow.Objects
{
    public interface IWowDynobject : IWowObject
    {
        ulong Caster { get; }

        float Radius { get; }

        int SpellId { get; }

        public new WowObjectType Type => WowObjectType.DynamicObject;
    }
}