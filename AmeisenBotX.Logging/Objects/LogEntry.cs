using AmeisenBotX.Logging.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Logging.Objects
{
    public class LogEntry
    {
        public LogEntry(LogLevel logLevel, string message, string callingClass, string callingFunction, int callingCodeline)
        {
            TimeStamp = DateTime.Now;
            LogLevel = logLevel;
            Message = message;
            CallingClass = callingClass;
            CallingFunction = callingFunction;
            CallingCodeline = callingCodeline;
        }

        public DateTime TimeStamp { get; private set; }

        public string Message { get; private set; }

        public LogLevel LogLevel { get; private set; }

        public string CallingClass { get; private set; }

        public string CallingFunction { get; private set; }

        public int CallingCodeline { get; private set; }

        public override string ToString()
        {
            return $"[{TimeStamp.ToLongTimeString()}] {("["+LogLevel.ToString()+"]").PadRight(9)} [{CallingClass}:{CallingCodeline}:{CallingFunction}] {Message}";
        }
    }
}
