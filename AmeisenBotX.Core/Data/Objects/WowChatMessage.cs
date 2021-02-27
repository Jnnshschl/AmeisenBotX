using AmeisenBotX.Core.Data.Enums;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Data.Objects
{
    public class WowChatMessage
    {
        public WowChatMessage(WowChat type, long timestamp, List<string> args)
        {
            Type = type;
            Timestamp = timestamp;
            Author = args[1];
            Channel = args[3];
            Flags = args[5];
            Language = args[2];
            Message = args[0];
        }

        public string Author { get; set; }

        public string Channel { get; set; }

        public string Flags { get; set; }

        public string Language { get; set; }

        public string Message { get; set; }

        public long Timestamp { get; set; }

        public WowChat Type { get; set; }

        public override string ToString()
        {
            return $"[{Type}]{(Channel.Length > 0 ? $"[{Channel}]" : "[]")}{(Flags.Length > 0 ? $"[{Flags}]" : "")}{(Language.Length > 0 ? $"[{Language}]" : "[]")} {Author}: {Message}";
        }
    }
}