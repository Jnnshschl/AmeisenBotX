namespace AmeisenBotX.Core.Data.Objects
{
    public interface IWowCorpse : IWowObject
    {
        int DisplayId { get; }

        ulong Owner { get; }

        ulong Party { get; }
    }
}