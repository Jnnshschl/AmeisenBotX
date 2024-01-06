using AmeisenBotX.Common.Keyboard.Enums;
using System;
using System.Runtime.InteropServices;

namespace AmeisenBotX.Common.Keyboard.Objects
{
    [StructLayout(LayoutKind.Sequential)]
    public struct LowLevelKeyboardInput
    {
        /// <summary>
        /// A virtual-key code. The code must be a value in the range 1 to 254.
        /// </summary>
        public KeyCode VirtualCode { get; set; }

        /// <summary>
        /// A hardware scan code for the key.
        /// </summary>
        public int HardwareScanCode { get; set; }

        /// <summary>
        /// The extended-key flag, event-injected Flags, context code, and transition-state flag.
        /// This member is specified as follows. An application can use the following values to test
        /// the keystroke Flags. Testing LLKHF_INJECTED (bit 4) will tell you whether the event was
        /// injected. If it was, then testing LLKHF_LOWER_IL_INJECTED (bit 1) will tell you whether
        /// or not the event was injected from a process running at lower integrity level.
        /// </summary>
        public int Flags { get; set; }

        /// <summary>
        /// The time stamp stamp for this message, equivalent to what GetMessageTime would return
        /// for this message.
        /// </summary>
        public int TimeStamp { get; set; }

        /// <summary>
        /// Additional information associated with the message.
        /// </summary>
        public nint AdditionalInformation { get; set; }
    }
}