using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects;
using System;
using System.Collections.Generic;
using System.IO;

namespace AmeisenBotX.Core.Chat
{
    public class ChatManager
    {
        public ChatManager(AmeisenBotConfig config, string dataPath)
        {
            Config = config;
            DataPath = dataPath;
            ChatMessages = new List<WowChatMessage>();
        }

        public event Action<WowChatMessage> OnNewChatMessage;

        public List<WowChatMessage> ChatMessages { get; }

        private AmeisenBotConfig Config { get; }

        private string DataPath { get; }

        public string ProtocolName(string type)
        {
            return $"{DataPath}\\\\chatprotocols\\\\chat-{type}-{DateTime.Now:dd-M-yyyy}.txt";
        }

        public bool TryParseMessage(ChatMessageType type, long timestamp, List<string> args)
        {
            if (args.Count < 6)
            {
                return false;
            }

            WowChatMessage chatMessage = new WowChatMessage(type, timestamp, args);
            ChatMessages.Add(chatMessage);

            if (Config.ChatProtocols)
            {
                try
                {
                    string typeName = chatMessage.Type switch
                    {
                        ChatMessageType.ADDON => "misc",
                        ChatMessageType.CHANNEL => "channel",
                        ChatMessageType.DND => "misc",
                        ChatMessageType.FILTERED => "filtered",
                        ChatMessageType.GUILD => "guild",
                        ChatMessageType.GUILD_ACHIEVEMENT => "guild",
                        ChatMessageType.IGNORED => "misc",
                        ChatMessageType.MONSTER_EMOTE => "npc",
                        ChatMessageType.MONSTER_PARTY => "npc",
                        ChatMessageType.MONSTER_SAY => "npc",
                        ChatMessageType.MONSTER_WHISPER => "npc",
                        ChatMessageType.MONSTER_YELL => "npc",
                        ChatMessageType.RAID_BOSS_EMOTE => "npc",
                        ChatMessageType.RAID_BOSS_WHISPER => "npc",
                        ChatMessageType.SYSTEM => "system",
                        _ => "normal",
                    };

                    string protocolName = ProtocolName(typeName);
                    string dirName = Path.GetDirectoryName(protocolName);

                    if (!Directory.Exists(dirName))
                    {
                        Directory.CreateDirectory(dirName);
                    }

                    File.AppendAllText(protocolName, $"{chatMessage}\n");
                }
                catch { }
            }

            OnNewChatMessage?.Invoke(chatMessage);

            return true;
        }
    }
}