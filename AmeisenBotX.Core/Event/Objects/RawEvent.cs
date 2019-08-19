using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.Event.Objects
{
    public struct RawEvent
    {
        public string @event;
        public List<string> args;
        public long time;
    }
}
