using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace AmeisenBotX.Core.Chat
{
    public class ChatManager
    {
        public List<WowChatMessage> ChatMessages { get; }

        public ChatManager()
        {
            ChatMessages = new List<WowChatMessage>();
        }

        public bool TryParseMessage(ChatMessageType type, long timestamp, List<string> args)
        {
            if (args.Count < 6)
            {
                return false;
            }

            ChatMessages.Add(new WowChatMessage(type, timestamp, args));

            return true;
        }
    }
}
