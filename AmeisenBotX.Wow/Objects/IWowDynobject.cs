namespace AmeisenBotX.Core.Data.Objects
{
    public interface IWowDynobject : IWowObject
    {
        ulong Caster { get; }

        float Radius { get; }

        int SpellId { get; }
    }
}