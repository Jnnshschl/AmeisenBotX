using Newtonsoft.Json;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Event.Objects
{
    public class RawEvent
    {
        [JsonProperty("event")]
        public string EventName { get; set; }

        [JsonProperty("args")]
        public List<string> Arguments { get; set; }

        [JsonProperty("time")]
        public long Timestamp { get; set; }
    }
}