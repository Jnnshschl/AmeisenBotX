﻿using AmeisenBotX.Common.Utils;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Collections.Generic;
using System.IO;

namespace AmeisenBotX.Core.Managers.Chat
{
    public class DefaultChatManager(AmeisenBotConfig config, string dataPath) : IChatManager
    {
        public event Action<WowChatMessage> OnNewChatMessage;

        public List<WowChatMessage> ChatMessages { get; } = [];

        private AmeisenBotConfig Config { get; } = config;

        private string DataPath { get; } = dataPath;

        public string ProtocolName(string type)
        {
            return $"{DataPath}\\\\chatprotocols\\\\chat-{type}-{DateTime.Now:dd-M-yyyy}.txt";
        }

        public bool TryParseMessage(WowChat type, long timestamp, List<string> args)
        {
            if (args.Count < 6)
            {
                return false;
            }

            WowChatMessage chatMessage = new(type, timestamp, args);
            ChatMessages.Add(chatMessage);

            if (Config.ChatProtocols)
            {
                try
                {
                    string typeName = chatMessage.Type switch
                    {
                        WowChat.ADDON => "misc",
                        WowChat.CHANNEL => "channel",
                        WowChat.DND => "misc",
                        WowChat.FILTERED => "filtered",
                        WowChat.GUILD => "guild",
                        WowChat.GUILD_ACHIEVEMENT => "guild",
                        WowChat.IGNORED => "misc",
                        WowChat.MONSTER_EMOTE => "npc",
                        WowChat.MONSTER_PARTY => "npc",
                        WowChat.MONSTER_SAY => "npc",
                        WowChat.MONSTER_WHISPER => "npc",
                        WowChat.MONSTER_YELL => "npc",
                        WowChat.RAID_BOSS_EMOTE => "npc",
                        WowChat.RAID_BOSS_WHISPER => "npc",
                        WowChat.SYSTEM => "system",
                        _ => "normal",
                    };

                    string protocolName = ProtocolName(typeName);
                    string dirName = Path.GetDirectoryName(protocolName);
                    IOUtils.CreateDirectoryIfNotExists(dirName);
                    File.AppendAllText(protocolName, $"{chatMessage}\n");
                }
                catch { }
            }

            OnNewChatMessage?.Invoke(chatMessage);

            return true;
        }
    }
}