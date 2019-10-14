using System.Collections.Generic;

namespace AmeisenBotX.Core.Event.Objects
{
    public struct RawEvent
    {
        public string @event;

        public List<string> args;

        public long time;
    }
}