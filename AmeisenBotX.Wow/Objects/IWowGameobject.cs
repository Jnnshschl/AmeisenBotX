using AmeisenBotX.Wow.Objects.Enums;
using System.Collections.Specialized;

namespace AmeisenBotX.Core.Data.Objects
{
    public interface IWowGameobject : IWowObject
    {
        byte Bytes0 { get; }

        ulong CreatedBy { get; }

        int DisplayId { get; }

        int Faction { get; }

        BitVector32 Flags { get; }

        WowGameobjectType GameobjectType { get; }

        int Level { get; }
    }
}