using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Chat
{
    public interface IChatManager
    {
        event Action<WowChatMessage> OnNewChatMessage;

        List<WowChatMessage> ChatMessages { get; }

        bool TryParseMessage(WowChat type, long timestamp, List<string> args);
    }
}