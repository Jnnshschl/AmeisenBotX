using System;
using System.Collections.Generic;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;

namespace AmeisenBotX.Core.Managers.Chat
{
    public interface IChatManager
    {
        event Action<WowChatMessage> OnNewChatMessage;

        List<WowChatMessage> ChatMessages { get; }

        bool TryParseMessage(WowChat type, long timestamp, List<string> args);
    }
}