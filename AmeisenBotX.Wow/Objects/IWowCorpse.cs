using AmeisenBotX.Wow.Objects.Enums;

namespace AmeisenBotX.Wow.Objects
{
    public interface IWowCorpse : IWowObject
    {
        int DisplayId { get; }

        ulong Owner { get; }

        ulong Party { get; }

        public new WowObjectType Type => WowObjectType.Corpse;
    }
}