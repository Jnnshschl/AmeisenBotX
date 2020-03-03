using Newtonsoft.Json;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Event.Objects
{
    public class WowEvent
    {
        [JsonProperty("args")]
        public List<string> Arguments { get; set; }

        [JsonProperty("event")]
        public string Name { get; set; }

        [JsonProperty("time")]
        public long Timestamp { get; set; }
    }
}